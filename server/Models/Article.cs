namespace server.Models
{
    //
    // ======================================
    // Article Model
    // ======================================
    //
    // Purpose:
    // - Represents a single news article fetched from an API or stored in the database.
    // - Stores full metadata about the article, including its source, author, title, content, and category.
    //
    public class Article
    {
        // --------------------------
        // Properties
        // --------------------------

        public Source? Source { get; set; }       // The source object containing ID and name of the news provider.
        public string? Author { get; set; }       // The author of the article (optional).
        public string Title { get; set; }         // The title of the article.
        public string? Description { get; set; }  // A short description or summary of the article (optional).
        public string Url { get; set; }           // The direct URL to the full article.
        public string? UrlToImage { get; set; }   // The URL to the article's image (optional).
        public string PublishedAt { get; set; }   // The publication date/time of the article.
        public string? Content { get; set; }      // The full content of the article (optional).
        public string? Category { get; set; }     // The article's category (e.g., Technology, Health).

        // --------------------------
        // Constructors
        // --------------------------

        // Default constructor (used when creating an empty article object before assigning values).
        public Article() { }

        // Full constructor (used when all article details are known at creation time).
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
