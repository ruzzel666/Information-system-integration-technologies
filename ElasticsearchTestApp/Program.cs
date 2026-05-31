
using Confluent.Kafka;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using ElasticsearchTestApp.Data;
using ElasticsearchTestApp.Services;
using Microsoft.EntityFrameworkCore;

namespace ElasticsearchTestApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var connectionString = builder.Configuration.GetConnectionString("ArticleDbConnection");
            builder.Services.AddDbContext<ArticleDbContext>(options =>
            {
                options.UseMySql(connectionString, ServerVersion.Parse("11.2-mariadb"), mySqlOptions =>
                {
                    mySqlOptions.EnableRetryOnFailure();
                });
            });

            // Add services to the container.
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var settings = new ElasticsearchClientSettings(new Uri("http://localhost:9200"))
                .Authentication(new BasicAuthentication("elastic", "elastic_password"))
                .DefaultIndex("articles");

            // Клиент регистрируется как Singleton
            builder.Services.AddSingleton(new ElasticsearchClient(settings));

            builder.Services.AddSingleton(new ProducerConfig
            {
                BootstrapServers = "localhost:9094"
            });
            builder.Services.AddSingleton<ArticleKafkaProducer>();

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
