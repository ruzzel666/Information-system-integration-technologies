
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using ElasticsearchTestApp.Services;
using RabbitMQ.Client;

namespace ElasticsearchTestApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var settings = new ElasticsearchClientSettings(new Uri("http://localhost:9200"))
                .Authentication(new BasicAuthentication("elastic", "elastic_password"))
                .DefaultIndex("articles");

            // Клиент регистрируется как Singleton
            builder.Services.AddSingleton(new ElasticsearchClient(settings));

            #region -- RabbitMQ
            builder.Services.AddSingleton(new ConnectionFactory
            {
                HostName = "localhost",
                Port = 5672,
                UserName = "rabbituser",
                Password = "rabbitpassword"
            });

            builder.Services.AddSingleton<ArticleRabbitMqProducer>();
            builder.Services.AddHostedService<RabbitMqToElasticHostedService>();
            #endregion

            builder.Services.AddScoped<ArticleSearchService>();

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
