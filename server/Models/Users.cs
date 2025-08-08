using server.DAL;

namespace server.Models
{
    // ==========================
    // Users Model
    // ==========================
    // Purpose:
    // - Represents a user in the system (both regular and admin users).
    // - Holds user properties such as name, email, password, and status.
    // - Contains methods that directly interact with the DBservicesUser class
    //   to perform database operations (CRUD and authentication).
   
    public class Users
    {
        // Primary key in the database (unique user identifier)
        public int Id { get; set; }

        // User's full name
        public string Name { get; set; } = string.Empty;

        // User's email address (used for login)
        public string Email { get; set; } = string.Empty;

        // User's password (should be stored hashed in DB for security)
        public string Password { get; set; } = string.Empty;

        // Indicates whether the user account is active (true) or disabled (false)
        public bool Active { get; set; }

        // Database service object for user-related operations
        private readonly DBservicesUser db = new DBservicesUser();

        // --------------------------
        // Constructors
        // --------------------------

        // Default constructor (required for model binding and serialization)
        public Users() { }

        // Parameterized constructor for quickly creating a user object
        public Users(int id, string name, string email, string password, bool active)
        {
            Id = id;
            Name = name;
            Email = email;
            Password = password;
            Active = active;
        }

        // --------------------------
        // Instance Methods (require an object of Users to run)
        // --------------------------

        // Inserts the current user into the database (registration)
        public int Register()
        {
            return db.InsertUser(this);
        }

        // Logs in a user by checking email and password against the database
        // Returns a Users object if credentials are valid, otherwise null
        public Users? Login(string email, string password)
        {
            return db.LoginUser(email, password);
        }

        // Updates all details for the current user in the database
        public int Update()
        {
            return db.UpdateUser(this);
        }

        // Updates only the "Active" status for the current user
        public int UpdateStatus(bool active)
        {
            DBservicesUser db = new DBservicesUser();
            return db.UpdateUserStatus(this.Id, active);
        }

        // Soft deletes the user by email (marks them as inactive instead of removing from DB)
        public int SoftDeleteByEmail(string email)
        {
            return db.SoftDeleteUserByEmail(email);
        }

        // --------------------------
        // Static Methods (can be called without creating a Users object)
        // --------------------------

        // Retrieves all active users from the database
        public static List<Users> Read()
        {
            DBservicesUser db = new DBservicesUser();
            return db.ReadAllUsers();
        }

        // Retrieves all users (including admins and inactive users) for admin panel
        public static List<Users> ReadAdmin()
        {
            DBservicesUser db = new DBservicesUser();
            return db.ReadAllUsersAdmin();
        }
    }
}
