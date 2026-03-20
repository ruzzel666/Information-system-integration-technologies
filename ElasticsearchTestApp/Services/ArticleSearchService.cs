using Elastic.Clients.Elasticsearch;
using ElasticsearchTestApp.Models;

namespace ElasticsearchTestApp.Services
{
    public class ArticleSearchService
    {
        private readonly ElasticsearchClient _client;

        public ArticleSearchService(ElasticsearchClient client)
        {
            _client = client;
        }

        public async Task IndexAsync(IEnumerable<ArticleDocument> documents)
        {
            // Bulk-индексация - предпочтительный способ записи
            var response = await _client.BulkAsync(b => b
                .Index("articles")
                .IndexMany(documents)
            );

            if (response.Errors)
            {
                throw new InvalidOperationException("Ошибка при индексации документов");
            }
        }

        public async Task<IReadOnlyCollection<ArticleDocument>> SearchAsync(string query)
        {
            var response = await _client.SearchAsync<ArticleDocument>(s => s
                .Indices("articles")
                .Query(q => q
                    .MultiMatch(m => m
                        .Fields(new[] { "title", "content" })
                        .Query(query)
                    )
                )
            );

            return response.Documents;
        }
    }
}
