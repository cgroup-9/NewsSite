using server.DAL;
using System;

namespace server.Models
{
    public class SharedArticleIndex
    {
        public int SharedId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string ArticleUrl { get; set; }
        public string Title { get; set; }
        public string UrlToImage { get; set; }
        public string Author { get; set; }
        public string Comment { get; set; }
        public DateTime DateShared { get; set; }

        public SharedArticleIndex() { }

        public SharedArticleIndex(int sharedId, int userId, string userName, string articleUrl,
                                string title, string urlToImage, string author, string comment, DateTime DateShared)
        {
            SharedId = sharedId;
            UserId = userId;
            UserName = userName;
            ArticleUrl = articleUrl;
            Title = title;
            UrlToImage = urlToImage;
            Author = author;
            Comment = comment;
            DateShared = DateShared;
        }

        public static List<SharedArticleIndex> GetAllSharedArticles(string? hiddenUserIds = null, int page = 1, int pageSize = 20)
        {
            DBservicesSharedArticles db = new DBservicesSharedArticles();
            return db.GetSharedArticles(hiddenUserIds, page, pageSize);
        }

    }
}
