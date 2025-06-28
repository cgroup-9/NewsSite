using server.DAL;

namespace server.Models
{
    public class SavedArticle
    {
        public int UserId { get; set; }
        public string ArticleUrl { get; set; }

        public SavedArticle() { }

        public SavedArticle(int userId, string articleUrl)
        {
            UserId = userId;
            ArticleUrl = articleUrl;
        }
    }
}
