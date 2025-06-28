using server.DAL;

namespace server.Models
{
    public class Article
    {
        public Source? Source { get; set; }
        public string? Author { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public string Url { get; set; }
        public string? UrlToImage { get; set; } 
        public string PublishedAt { get; set; }
        public string? Content { get; set; }
        public string? Category { get; set; }

        public Article() { }
        public Article(Source? source, string? author, string title, string? description, string url, string? urlToImage, string publishedAt, string? content)
        {
            Source = source;
            Author = author;
            Title = title;
            Description = description;
            Url = url;
            UrlToImage = urlToImage;
            PublishedAt = publishedAt;
            Content = content;
        }

    }
}
