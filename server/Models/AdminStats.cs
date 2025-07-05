using server.DAL;

namespace server.Models
{
    public class AdminStats
    {
        public string Date { get; set; }
        public int LoginCounter { get; set; }
        public int ApiFetchCounter { get; set; }
        public int SavedNewsCounter { get; set; }

        public AdminStats() { }

        public static AdminStats GetStats()
        {
            DBservicesUser db = new DBservicesUser();
            return db.GetAdminStats();
        }
    }
}
