using Microsoft.AspNetCore.Mvc;
using server.Models;

namespace server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        // POST: api/User/register
        [HttpPost("register")]
        public IActionResult Register([FromBody] Users user)
        {
            int result = user.Register();

            if (result == 0)
                return Ok(true);
            else if (result == 3)
                return BadRequest("Username or email already exists.");
            else
                return BadRequest("Unknown error occurred.");
        }

        // POST: api/Users/login
        [HttpPost("login")]
        public IActionResult Login([FromBody] Users loginUser)
        {
            Users user = new Users();
            Users? existingUser = user.Login(loginUser.Email, loginUser.Password);

            if (existingUser == null)
                return Unauthorized("Invalid email or password.");

            return Ok(existingUser);
        }

        // GET: api/Users
        [HttpGet]
        public ActionResult<IEnumerable<Users>> GetAllUsers()
        {
            return Ok(Users.Read());
        }

        // GET: api/Users/getAllUseresAdmin
        [HttpGet("getAllUseresAdmin")]
        public ActionResult<IEnumerable<Users>> GetAllUsersAdmin()
        {
            return Ok(Users.ReadAdmin());
        }

        // GET: api/Users/{id}
        [HttpGet("{id}")]
        public ActionResult<string> GetUserById(int id)
        {
            return Ok($"You requested user with ID = {id}");
        }

        // PUT: api/Users/{id}
        [HttpPut("{id}")]
        public IActionResult PutUserById(int id, [FromBody] string value)
        {
            return Ok($"You updated user {id} with value = {value}");
        }

        // DELETE: api/Users/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteUserById(int id)
        {
            return Ok($"You deleted user with ID = {id}");
        }

        // PUT: api/Users/update-user
        [HttpPut("update-user")]
        public IActionResult UpdateUser([FromBody] Users user)
        {
            int result = user.Update();

            if (result == 0)
                return Ok(new { message = "User updated successfully." });
            else if (result == 1)
                return NotFound(new { message = "User not found or deleted." });
            else
                return BadRequest(new { message = "Unknown error occurred." });
        }

        // PUT: api/Users/update-status
        [HttpPut("update-status")]
        public async Task<IActionResult> UpdateUserStatus([FromBody] UserStatusUpdateRequest request)
        {
            try
            {
                Users u = new Users { Id = request.Id };
                int res = u.UpdateStatus(request.Active);
                return Ok(new { message = "Status updated" });
            }
            catch (Exception ex)
            {
                return BadRequest("Error: " + ex.Message);
            }
        }

        // DELETE: api/Users/delete-by-email/{email}
        [HttpDelete("delete-by-email/{email}")]
        public IActionResult SoftDeleteUserByEmail(string email)
        {
            Users user = new Users();
            int result = user.SoftDeleteByEmail(email);

            if (result == 0)
                return Ok("User deleted successfully.");
            else if (result == 1)
                return NotFound("User not found or already deleted.");
            else
                return BadRequest("Unknown error occurred.");
        }

        // GET: api/Users/stats
        [HttpGet("stats")]
        public IActionResult GetAdminStats()
        {
            try
            {
                AdminStats stats = AdminStats.GetStats(); 
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Failed to fetch admin stats: " + ex.Message);
            }
        }
    }
}
