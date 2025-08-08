using server.DAL;

namespace server.Models
{
    //
    // ================================
    // SaveArticleRequest Model
    // ================================
    //
    // Purpose:
    // - Represents a request to save an article for a specific user.
    // - Contains all relevant article details along with the user ID.
    // - Provides methods to interact with the database via DBservicesSavedArticles.
    //
    public class SaveArticleRequest
    {
        // --------------------------
        // Properties
        // --------------------------

        // The ID of the user who is saving the article.
        public int UserId { get; set; }

        // The unique URL of the article (used as a primary identifier in many cases).
        public string ArticleUrl { get; set; }

        // The title of the article (optional, may be null).
        public string? Title { get; set; }

        // A short description or summary of the article (optional).
        public string? Description { get; set; }

        // The URL of the article's image (optional).
        public string? UrlToImage { get; set; }

        // The author of the article (optional).
        public string? Author { get; set; }

        // The published date/time of the article as a string (optional).
        // Note: This is stored as a string, not DateTime, to match the API's format.
        public string? PublishedAt { get; set; }

        // The full content/body of the article (optional).
        public string? Content { get; set; }

        // The category the article belongs to (e.g., "technology", "sports").
        public string? Category { get; set; }

        // --------------------------
        // Constructors
        // --------------------------

        // Default constructor (used when creating an empty object to populate later).
        public SaveArticleRequest() { }

        // Full constructor (used when all properties are known at object creation).
        public SaveArticleRequest(int userId, string articleUrl, string? title, string? description,string? urlToImage, string? author,string? publishedAt,string? content,string? category)
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

        // --------------------------
        // Database Interaction Methods
        // --------------------------

        // Saves this article for the specified user in the database.
        // Returns:
        // - An integer result from the stored procedure indicating success or failure.
        public int Save()
        {
            DBservicesSavedArticles db = new DBservicesSavedArticles();
            return db.SaveArticle(this);
        }

        // Deletes a saved article for the given user and article URL.
        // This is a static method because deletion is based on identifiers, not an object instance.
        public static int Delete(int userId, string articleUrl)
        {
            DBservicesSavedArticles db = new DBservicesSavedArticles();
            return db.DeleteSavedArticle(userId, articleUrl);
        }

        // Retrieves a paginated list of saved articles for a specific user.
        // Optional filters:
        // - categories: a comma-separated list of categories to include.
        // - searchTerm: keyword search in titles/descriptions.
        public static List<SaveArticleRequest> GetSavedArticles(
            int userId,
            int page,
            int pageSize,
            string? categories = null,
            string? searchTerm = null)
        {
            DBservicesSavedArticles db = new DBservicesSavedArticles();
            return db.GetSavedArticles(userId, page, pageSize, categories, searchTerm);
        }
    }
}
