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

    }
}
