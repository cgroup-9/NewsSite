using server.DAL;

namespace server.Models
{
    public class ReportSharedArticleRequest
    {
        public int ReporterUserId { get; set; }
        public int SharedArticleId { get; set; }

        public ReportSharedArticleRequest() { }

        public ReportSharedArticleRequest(int reporterUserId, int sharedArticleId)
        {
            ReporterUserId = reporterUserId;
            SharedArticleId = sharedArticleId;
        }

        public int Report()
        {
            DBservicesSharedArticles db = new DBservicesSharedArticles();
            return db.ReportSharedArticle(this);
        }

    }
}
