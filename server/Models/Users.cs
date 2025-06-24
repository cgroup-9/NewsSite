using server.DAL;

namespace server.Models
{
    public class Users
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool Active { get; set; } = true;

        private readonly DBservicesUser db = new DBservicesUser();

        public Users() { }

        public Users(string name, string email, string password, bool active = true)
        {
            Name = name;
            Email = email;
            Password = password;
            Active = active;
        }

        public int Register()
        {
            return db.InsertUser(this);  
        }

        public Users? Login(string email, string password)
        {
            return db.LoginUser(email, password); 
        }
        public static List<Users> Read()
        {
            DBservicesUser db = new DBservicesUser();
            return db.ReadAllUsers();
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
