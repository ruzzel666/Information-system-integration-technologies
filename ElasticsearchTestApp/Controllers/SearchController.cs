using ElasticsearchTestApp.Models;
using ElasticsearchTestApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace ElasticsearchTestApp.Controllers
{
    [ApiController]
    [Route("api/search")]
    public class SearchController : ControllerBase
    {
        private readonly ArticleSearchService _searchService;
        private readonly ArticleKafkaProducer _kafkaProducer;

        public SearchController(ArticleSearchService searchService, ArticleKafkaProducer kafkaProducer)
        {
            _searchService = searchService;
            _kafkaProducer = kafkaProducer;
        }

        [HttpGet("index")]
        public async Task<IActionResult> Index()
        {
            var documents = new[]
            {
            new ArticleDocument
            {
                Id = 1,
                Title = "Введение в Elasticsearch",
                Content = "Моя первая статья по ASP.NET Core и Elasticsearch"
            },
            new ArticleDocument
            {
                Id = 2,
                Title = "Поиск в .NET",
                Content = "Полнотекстовый поиск в .NET"
            },
            new ArticleDocument
            {
                Id = 3,
                Title = "Elasticsearch 9",
                Content = "Работа с Elasticsearch 9 - быстрый старт"
            }
        };

            foreach (var doc in documents)
            {
                await _kafkaProducer.PublishAsync(doc);
            }

            return Accepted(new { Message = "Статьи отправлены в Kafka. Ожидается фоновая индексация через Kafka Connect." });
        }

        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] string q)
        {
            var result = await _searchService.SearchAsync(q);
            return Ok(result);
        }
    }

}
