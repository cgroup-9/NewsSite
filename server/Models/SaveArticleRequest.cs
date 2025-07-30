using server.DAL;

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

        // Default constructor
        public SaveArticleRequest() { }

        // Full constructor
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

        // Saves this article to the database for the current user
        public int Save()
        {
            DBservicesSavedArticles db = new DBservicesSavedArticles();
            return db.SaveArticle(this);
        }

        // Deletes this article from the user's saved list
        public static int Delete(int userId, string articleUrl)
        {
            DBservicesSavedArticles db = new DBservicesSavedArticles();
            return db.DeleteSavedArticle(userId, articleUrl);
        }

        // Retrieves all articles saved by a specific user, optionally filtered by categories
        public static List<SaveArticleRequest> GetSavedArticles(int userId, int page, int pageSize, string? categories = null)
        {
            DBservicesSavedArticles db = new DBservicesSavedArticles();
            return db.GetSavedArticles(userId, page, pageSize, categories);
        }

    }
}
