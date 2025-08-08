using server.DAL;

namespace server.Models
{
    //
    // ======================================
    // AdminStats Model
    // ======================================
    //
    // Purpose:
    // - Represents key statistics for the admin dashboard.
    // - Includes daily counts for logins, API fetches, and saved news.
    //
    public class AdminStats
    {
        // --------------------------
        // Properties
        // --------------------------

        public string Date { get; set; }         // The date for which the statistics are recorded.
        public int LoginCounter { get; set; }    // Number of user logins on this date.
        public int ApiFetchCounter { get; set; } // Number of API fetches made on this date.
        public int SavedNewsCounter { get; set; } // Number of saved news articles on this date.

        // --------------------------
        // Constructors
        // --------------------------

        public AdminStats() { } // Default constructor.

        // --------------------------
        // Database Interaction
        // --------------------------

        // Retrieves the latest admin statistics from the database.
        public static AdminStats GetStats()
        {
            DBservicesUser db = new DBservicesUser();
            return db.GetAdminStats();
        }
    }
}
