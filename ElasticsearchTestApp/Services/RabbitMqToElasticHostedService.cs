using System.Text;
using System.Text.Json;
using Elastic.Clients.Elasticsearch;
using ElasticsearchTestApp.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ElasticsearchTestApp.Services
{
    public class RabbitMqToElasticHostedService : BackgroundService
    {
        private readonly ConnectionFactory _connectionFactory;
        private readonly ElasticsearchClient _elasticClient;
        private IConnection? _connection;
        private IChannel? _channel;

        private const string QueueName = "article-index-queue";

        public RabbitMqToElasticHostedService(
            ConnectionFactory connectionFactory,
            ElasticsearchClient elasticClient)
        {
            _connectionFactory = connectionFactory;
            _elasticClient = elasticClient;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _connection = await _connectionFactory.CreateConnectionAsync(stoppingToken);
            _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

            // Гарантируем, что очередь существует перед тем, как начать ее слушать
            await _channel.QueueDeclareAsync(
                queue: QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: stoppingToken
            );

            // Создаем асинхронного потребителя
            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                var document = JsonSerializer.Deserialize<ArticleDocument>(message);

                if (document != null)
                {
                    try
                    {
                        // Идемпотентная запись (upsert) в Elasticsearch. 
                        // Метод явно указывает ID. Если документ с таким ID уже есть, он обновится.
                        var response = await _elasticClient.IndexAsync(document, i => i
                            .Id(document.Id)
                            .Index("articles")
                        );

                        if (response.IsValidResponse)
                        {
                            // Успех! Подтверждаем RabbitMQ, что сообщение обработано и его можно удалить
                            await _channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
                        }
                        else
                        {
                            // Ошибка Elasticsearch. Сообщаем брокеру, чтобы он вернул сообщение в очередь (requeue)
                            await _channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
                        }
                    }
                    catch (Exception)
                    {
                        // В случае непредвиденной ошибки также возвращаем сообщение в очередь
                        await _channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
                    }
                }
            };

            // Запускаем прослушивание. Важно: autoAck = false, мы подтверждаем вручную!
            await _channel.BasicConsumeAsync(
                queue: QueueName,
                autoAck: false,
                consumer: consumer,
                cancellationToken: stoppingToken
            );

            // Оставляем задачу висеть в фоне до тех пор, пока приложение не остановят
            try
            {
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                // Ожидаемое поведение при остановке хоста
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_channel is not null) await _channel.CloseAsync(cancellationToken);
            if (_connection is not null) await _connection.CloseAsync(cancellationToken);
            await base.StopAsync(cancellationToken);
        }
    }
}
