namespace ElasticsearchTestApp.Models
{
    public class ArticleDocument
    {
        public int Id { get; set; }

        // Новое поле для названия статьи
        public string Title { get; set; } = string.Empty;

        // Основное поле для полнотекстового поиска
        public string Content { get; set; } = string.Empty;
    }
}
