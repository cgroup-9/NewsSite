namespace server.Models
{
    //
    // ======================================
    // LoginStats Model
    // ======================================
    //
    // Purpose:
    // - Represents daily login statistics for the system.
    // - Used mainly for analytics, admin dashboards, or AI-based data analysis.
    // - Typically retrieved from aggregated data in the Admin_FP table or similar.
    //
    public class LoginStats
    {
        // --------------------------
        // Properties
        // --------------------------

        // The date for which the login count was recorded.
        // Example: 2025-08-08 means the logins counted are for that day.
        public DateTime Date { get; set; }

        // The number of logins that occurred on the given date.
        public int LoginCounter { get; set; }
    }
}
