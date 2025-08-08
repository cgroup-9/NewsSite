namespace server.Models
{
    //
    // ===========================
    // SavedStats Model
    // ===========================
    //
    // Purpose:
    // - Represents statistics about a saved article.
    // - Used primarily for admin analytics and reporting.
    // - Retrieved from the database (e.g., when analyzing user activity over time).
    //
    public class SavedStats
    {
        // --------------------------
        // Properties
        // --------------------------

        // The unique ID of the user who saved the article.
        public int UserId { get; set; }

        // The unique ID of the article that was saved.
        public int ArticleId { get; set; }

        // The date and time when the article was saved by the user.
        public DateTime DateSaved { get; set; }

        // The category of the saved article (e.g., "technology", "health").
        public string Category { get; set; }
    }
}
