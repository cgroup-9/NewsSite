using server.DAL;

namespace server.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool Active { get; set; } = true;

        public User() { }

        public User(string name, string email, string password, bool active = true)
        {
            Name = name;
            Email = email;
            Password = password;
            Active = active;
        }

        public int Register()
        {
            DBservicesUser db = new DBservicesUser();
            return db.InsertUser(this);  
        }

        public User? Login(string name, string password)
        {
            DBservicesUser db = new DBservicesUser();
            return db.LoginUser(name, password); 
        }
    }
}
