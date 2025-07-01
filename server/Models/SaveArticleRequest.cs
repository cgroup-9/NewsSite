namespace server.Models
{
    public class SaveArticleRequest
    {
        public int UserId { get; set; }
        public string ArticleUrl { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? UrlToImage { get; set; }
        public string? Author { get; set; }
        public string? PublishedAt { get; set; }
        public string? Content { get; set; }
        public string? Category { get; set; }

        public SaveArticleRequest() { }

        public SaveArticleRequest(int userId, string articleUrl, string? title, string? description,
                            string? urlToImage, string? author, string? publishedAt,
                            string? content, string? category)
        {
            UserId = userId;
            ArticleUrl = articleUrl;
            Title = title;
            Description = description;
            UrlToImage = urlToImage;
            Author = author;
            PublishedAt = publishedAt;
            Content = content;
            Category = category;
        }
    }
}
