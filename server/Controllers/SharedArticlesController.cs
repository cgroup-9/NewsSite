using Microsoft.AspNetCore.Mvc;
using server.Models;

namespace server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SharedArticleController : ControllerBase
    {
        // POST: api/sharedarticle
        [HttpPost]
        public IActionResult ShareArticle([FromBody] SharedArticle shared)
        {
            try
            {
                int result = shared.Share();
                if (result == 1)
                    return Ok(new { message = "Article shared successfully." });
                return BadRequest(new { message = "Failed to share article." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }

        // GET: api/sharedarticle
        [HttpGet]
        public IActionResult GetSharedArticles()
        {
            try
            {
                var list = SharedArticle.GetAllSharedArticles();
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }
    }
}
