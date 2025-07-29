namespace server.Models
{
    public class ReportedCommentsAdminPanel
    {
        public int ReportId { get; set; }
        public string Comment { get; set; }
        public string SharedByUserName { get; set; }
        public int SharedByUserId { get; set; }
        public DateTime ReportDate { get; set; }
    }
}
