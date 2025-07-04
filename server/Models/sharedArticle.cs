using server.DAL;

namespace server.Models
{
    public class SharedArticle
    {
        public int UserId { get; set; }
        public string ArticleUrl { get; set; }
        public string Comment { get; set; }

        public SharedArticle() { }

        public SharedArticle(int userId, string articleUrl, string comment)
        {
            UserId = userId;
            ArticleUrl = articleUrl;
            Comment = comment;
        }

        public int Share()
        {
            DBservicesSharedArticles db = new DBservicesSharedArticles();
            return db.ShareArticle(this);
        }

        public static List<SharedArticle> GetAllSharedArticles()
        {
            DBservicesSharedArticles db = new DBservicesSharedArticles();
            return db.GetSharedArticles();
        }
    }
}
