namespace server.Models
{
    //
    // ======================================
    // ActivityData Model
    // ======================================
    //
    // Purpose:
    // - Represents combined daily activity statistics.
    // - Tracks both login count and saved news count for a given date.
    //
    public class ActivityData
    {
        public DateTime Date { get; set; }       // The date of the recorded activity.
        public int LoginCounter { get; set; }    // Number of logins that occurred on this date.
        public int SavedNewsCounter { get; set; } // Number of news articles saved on this date.
    }
}
