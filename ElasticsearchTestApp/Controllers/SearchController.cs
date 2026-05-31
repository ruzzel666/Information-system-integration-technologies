using ElasticsearchTestApp.Data;
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

        private readonly ArticleDbContext _context;

        public SearchController(ArticleSearchService searchService, ArticleKafkaProducer kafkaProducer, ArticleDbContext context)
        {
            _searchService = searchService;
            _kafkaProducer = kafkaProducer;
            _context = context;
        }

        [HttpPost("index")]
        public async Task<IActionResult> Index([FromBody] ArticleDocument doc)
        {
            await _context.ArticleDocuments.AddAsync(doc);
            await _context.SaveChangesAsync();
            // Мы сохранили только в БД. Остальное сделает Debezium.
            return Accepted(new { Message = "Статья сохранена в БД." });
        }

        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] string q)
        {
            var result = await _searchService.SearchAsync(q);
            return Ok(result);
        }
    }

}
