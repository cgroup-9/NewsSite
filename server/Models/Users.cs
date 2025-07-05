using server.DAL;

namespace server.Models
{
    public class Users
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool Active { get; set; }

        private readonly DBservicesUser db = new DBservicesUser();

        public Users() { }

        public Users(int id, string name, string email, string password, bool active)
        {
            Id = id;
            Name = name;
            Email = email;
            Password = password;
            Active = active;
        }

        public int Register()
        {
            return db.InsertUser(this);
        }
        public static List<Users> Read()
        {
            DBservicesUser db = new DBservicesUser();
            return db.ReadAllUsers();
        }
        public static List<Users> ReadAdmin()
        {
            DBservicesUser db = new DBservicesUser();
            return db.ReadAllUsersAdmin();
        }
        public Users? Login(string email, string password)
        {
            return db.LoginUser(email, password);
        }

        public int Update()
        {
            return db.UpdateUser(this);
        }
        public int UpdateStatus(bool active)
        {
            DBservicesUser db = new DBservicesUser();
            return db.UpdateUserStatus(this.Id, active);
        }

        public int SoftDeleteByEmail(string email)
        {
            return db.SoftDeleteUserByEmail(email);
        }
    }
}
