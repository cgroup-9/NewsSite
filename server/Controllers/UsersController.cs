using Microsoft.AspNetCore.Mvc;  // Provides classes and attributes for building Web APIs (e.g., ControllerBase, Route, HttpGet, etc.)
using server.Models;            // Imports your Models namespace where the Users and AdminStats classes are defined

namespace server.Controllers
{
    // The route prefix for this controller will be "api/Users"
    [Route("api/[controller]")]
    [ApiController] // Indicates this is an API controller (automatic model validation, binding, etc.)
    public class UsersController : ControllerBase
    {
        // ================= REGISTER =================
        // POST: api/User/register
        // Registers a new user
        [HttpPost("register")]
        public IActionResult Register([FromBody] Users user) // Reads the user data from the request body (JSON -> Users object)
        {
            int result = user.Register(); // Calls the Register() method in the Users model to save the user to the database

            if (result == 0)
                return Ok(true); // Success
            else if (result == 3)
                return BadRequest("Username or email already exists."); // Duplicate
            else
                return BadRequest("Unknown error occurred."); // General error
        }

        // ================= LOGIN =================
        // POST: api/Users/login
        // Logs in a user by verifying email and password
        [HttpPost("login")]
        public IActionResult Login([FromBody] Users loginUser)
        {
            Users user = new Users();
            Users? existingUser = user.Login(loginUser.Email, loginUser.Password); // Checks credentials

            if (existingUser == null)
                return Unauthorized("Invalid email or password."); // Login failed

            return Ok(existingUser); // Login success, returns the user object
        }

        // ================= GET ALL USERS (BASIC) =================
        // GET: api/Users
        // Returns a list of all active users
        [HttpGet]
        public ActionResult<IEnumerable<Users>> GetAllUsers()
        {
            return Ok(Users.Read()); // Calls Users.Read() which fetches users from DB
        }

        // ================= GET ALL USERS (ADMIN VIEW) =================
        // GET: api/Users/getAllUseresAdmin
        // Returns all users, possibly with extra admin-only details
        [HttpGet("getAllUseresAdmin")]
        public ActionResult<IEnumerable<Users>> GetAllUsersAdmin()
        {
            return Ok(Users.ReadAdmin()); // Calls Users.ReadAdmin() for extended data
        }

        // ================= GET USER BY ID (TEST/DUMMY) =================
        // GET: api/Users/{id}
        [HttpGet("{id}")]
        public ActionResult<string> GetUserById(int id)
        {
            return Ok($"You requested user with ID = {id}"); // Currently just returns a test string
        }

        // ================= UPDATE USER BY ID (TEST/DUMMY) =================
        // PUT: api/Users/{id}
        [HttpPut("{id}")]
        public IActionResult PutUserById(int id, [FromBody] string value)
        {
            return Ok($"You updated user {id} with value = {value}"); // Placeholder for future update logic
        }

        // ================= DELETE USER BY ID (TEST/DUMMY) =================
        // DELETE: api/Users/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteUserById(int id)
        {
            return Ok($"You deleted user with ID = {id}"); // Placeholder for future delete logic
        }

        // ================= UPDATE USER (FULL) =================
        // PUT: api/Users/update-user
        // Updates all user details
        [HttpPut("update-user")]
        public IActionResult UpdateUser([FromBody] Users user)
        {
            int result = user.Update(); // Calls Update() in the Users model

            if (result == 0)
                return Ok(new { message = "User updated successfully." });
            else if (result == 1)
                return NotFound(new { message = "User not found or deleted." });
            else
                return BadRequest(new { message = "Unknown error occurred." });
        }

        // ================= UPDATE USER STATUS =================
        // PUT: api/Users/update-status
        // Activates/deactivates a user account
        [HttpPut("update-status")]
        public async Task<IActionResult> UpdateUserStatus([FromBody] UserStatusUpdateRequest request)
        {
            try
            {
                Users u = new Users { Id = request.Id };
                int res = u.UpdateStatus(request.Active); // Calls method to change Active flag in DB
                return Ok(new { message = "Status updated" });
            }
            catch (Exception ex)
            {
                return BadRequest("Error: " + ex.Message); // Catches unexpected errors
            }
        }

        // ================= SOFT DELETE USER BY EMAIL =================
        // DELETE: api/Users/delete-by-email/{email}
        // Marks a user as deleted without removing the record from DB
        [HttpDelete("delete-by-email/{email}")]
        public IActionResult SoftDeleteUserByEmail(string email)
        {
            Users user = new Users();
            int result = user.SoftDeleteByEmail(email);

            if (result == 0)
                return Ok("User deleted successfully."); // Success
            else if (result == 1)
                return NotFound("User not found or already deleted."); // Not found or already deleted
            else
                return BadRequest("Unknown error occurred."); // General error
        }

        // ================= GET ADMIN STATS =================
        // GET: api/Users/stats
        // Fetches usage statistics for the admin dashboard
        [HttpGet("stats")]
        public IActionResult GetAdminStats()
        {
            try
            {
                AdminStats stats = AdminStats.GetStats(); // Calls static method to get aggregated stats from DB
                return Ok(stats); // Returns stats object (Logins, API fetches, Saved articles, etc.)
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Failed to fetch admin stats: " + ex.Message); // Server error
            }
        }
    }
}
