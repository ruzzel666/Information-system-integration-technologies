using ElasticsearchTestApp.Models;
using ElasticsearchTestApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace ElasticsearchTestApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ArticleController : ControllerBase
    {
        private readonly ArticleRabbitMqProducer _producer;

        public ArticleController(ArticleRabbitMqProducer producer)
        {
            _producer = producer;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateArticle([FromBody] ArticleDocument article)
        {
            // 1. Отправляем событие в брокер сообщений
            await _producer.PublishArticleAsync(article);

            // 2. Возвращаем 202 Accepted
            // Это говорит клиенту: "Запрос принят в обработку, но она еще не завершена"
            return Accepted(new
            {
                Message = "Статья принята в обработку и поставлена в очередь на индексацию",
                ArticleId = article.Id
            });
        }
    }
}
