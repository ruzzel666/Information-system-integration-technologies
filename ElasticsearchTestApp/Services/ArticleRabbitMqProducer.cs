using ElasticsearchTestApp.Models;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace ElasticsearchTestApp.Services
{
    public class ArticleRabbitMqProducer
    {
        private readonly ConnectionFactory _connectionFactory;
        private IConnection? _connection;
        private IChannel? _channel;

        // Имя очереди для задач индексации
        private const string QueueName = "article-index-queue";

        public ArticleRabbitMqProducer(ConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task InitializeAsync()
        {
            _connection ??= await _connectionFactory.CreateConnectionAsync();

            if (_channel is null)
            {
                _channel = await _connection.CreateChannelAsync();

                // Объявляем очередь. Durable = true означает, что очередь переживет перезапуск брокера.
                await _channel.QueueDeclareAsync(
                    queue: QueueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null
                );
            }
        }

        public async Task PublishArticleAsync(ArticleDocument article)
        {
            if (_channel is null) await InitializeAsync();

            var json = JsonSerializer.Serialize(article);
            var body = Encoding.UTF8.GetBytes(json);

            // Отправляем статью в очередь
            await _channel.BasicPublishAsync(
                exchange: "",
                routingKey: QueueName,
                body: body
            );
        }
    }
}
