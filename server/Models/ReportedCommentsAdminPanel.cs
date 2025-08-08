namespace server.Models
{
    //
    // ======================================
    // ReportedCommentsAdminPanel Model
    // ======================================
    //
    // Purpose:
    // - Represents a single reported comment entry that will be shown in the Admin Panel.
    // - Contains all necessary data for the admin to review, such as:
    //   * The report ID (unique identifier for the report entry)
    //   * The comment text that was reported
    //   * Details of the user who originally shared the comment
    //   * The date and time when the comment was reported
    //
    public class ReportedCommentsAdminPanel
    {
        // --------------------------
        // Properties
        // --------------------------

        // Unique identifier for the report entry in the database.
        public int ReportId { get; set; }

        // The actual comment text that was reported by a user.
        public string Comment { get; set; }

        // The name of the user who originally shared this comment.
        public string SharedByUserName { get; set; }

        // The user ID of the person who shared the comment.
        public int SharedByUserId { get; set; }

        // The date and time when the report was created.
        public DateTime ReportDate { get; set; }
    }
}
