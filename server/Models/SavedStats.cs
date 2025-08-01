namespace server.Models
{
    public class SavedStats
    {
        public int UserId { get; set; }
        public int ArticleId { get; set; }
        public DateTime DateSaved { get; set; }
        public string Category { get; set; }
    }

}
