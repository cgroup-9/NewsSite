using Microsoft.AspNetCore.Mvc;
using server.DAL;
using server.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        // POST api/Users
        [HttpPost]
        public IActionResult Register([FromBody] User user)
        {
            try
            {
                int res = user.Register();
                if (res == 1)
                    return Ok(true);
                else if (res == 3)
                    return Conflict("Username or Email already exists.");
                else
                    return BadRequest("User registration failed");
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST: api/User/login
        [HttpPost("login")]
        public IActionResult Login([FromBody] User loginUser)
        {
            User user = new User();
            User? existingUser = user.Login(loginUser.Name, loginUser.Password);

            if (existingUser == null)
            {
                return Unauthorized("Invalid name or password.");
            }

            return Ok(existingUser);
        }


        // GET: api/<UsersController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<UsersController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }


        // PUT api/<UsersController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<UsersController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
