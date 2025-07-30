using Microsoft.AspNetCore.Mvc;
using server.DAL;
using server.Models;
using server.Models.server.Models;

namespace server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SavedArticleController : ControllerBase
    {
        // GET: api/savedarticle/{userId}?page=1&pageSize=20&categories=Sports,Health
        [HttpGet("{userId}")]
        public IActionResult GetSavedArticles(int userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? categories = null)
        {
            try
            {
                var savedArticles = SaveArticleRequest.GetSavedArticles(userId, page, pageSize, categories);
                return Ok(savedArticles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }



        // POST: api/savedarticle
        [HttpPost]
        public IActionResult SaveArticle([FromBody] SaveArticleRequest articleToSave)
        {
            try
            {
                int result = articleToSave.Save();
                if (result == 1)
                    return Ok(new { message = "Article saved successfully." });
                if (result == 0)
                    return Ok(new { message = "This article is already saved." });
                return BadRequest(new { message = "Unknown error occurred." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }

        // DELETE: api/savedarticle
        [HttpDelete]
        public IActionResult DeleteArticle([FromBody] ArticleDeleteRequest request)
        {
            try
            {
                int result = SaveArticleRequest.Delete(request.UserId, request.ArticleUrl);
                if (result == 1)
                    return Ok(new { message = "Article removed from saved list." });
                return NotFound(new { message = "Article not found or not saved by user." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }

    }
}
