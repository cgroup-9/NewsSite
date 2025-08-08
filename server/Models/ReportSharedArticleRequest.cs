using server.DAL;

namespace server.Models
{
    //
    // ======================================
    // ReportSharedArticleRequest Model
    // ======================================
    //
    // Purpose:
    // - Represents a request made by a user to report a specific shared article.
    // - Holds the IDs of both the reporter (user) and the shared article being reported.
    // - Provides a method to send the report request to the database via DBservicesSharedArticles.
    //
    public class ReportSharedArticleRequest
    {
        // --------------------------
        // Properties
        // --------------------------

        // The ID of the user who is reporting the shared article.
        public int ReporterUserId { get; set; }

        // The ID of the shared article being reported.
        public int SharedArticleId { get; set; }

        // --------------------------
        // Constructors
        // --------------------------

        // Default constructor (used when an empty object is needed before setting values).
        public ReportSharedArticleRequest() { }

        // Full constructor (used when both the reporter's user ID and shared article ID are known).
        public ReportSharedArticleRequest(int reporterUserId, int sharedArticleId)
        {
            ReporterUserId = reporterUserId;
            SharedArticleId = sharedArticleId;
        }

        // --------------------------
        // Database Interaction
        // --------------------------

        // Sends the report request to the database.
        // Returns:
        // - An integer result from the stored procedure indicating success or failure.
        public int Report()
        {
            DBservicesSharedArticles db = new DBservicesSharedArticles();
            return db.ReportSharedArticle(this);
        }
    }
}
