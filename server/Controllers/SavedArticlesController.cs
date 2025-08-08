using Microsoft.AspNetCore.Mvc;           // Provides API controller features
using server.DAL;                         // Data Access Layer (database logic)
using server.Models;                      // Contains the SaveArticleRequest and ArticleDeleteRequest models

namespace server.Controllers
{
    [Route("api/[controller]")] // Base route → "api/SavedArticle"
    [ApiController]             // Enables automatic model binding, validation, etc.
    public class SavedArticleController : ControllerBase
    {
        // ===================== GET SAVED ARTICLES =====================
        // GET: api/savedarticle/{userId}?page=1&pageSize=20&categories=Sports,Health&searchTerm=football
        // Retrieves a paginated list of saved articles for a given user, with optional filters
        [HttpGet("{userId}")]
        public IActionResult GetSavedArticles(
            int userId,                                // User ID whose saved articles to fetch
            [FromQuery] int page = 1,                  // Page number (default: 1)
            [FromQuery] int pageSize = 20,             // Number of articles per page (default: 20)
            [FromQuery] string? categories = null,     // Optional filter: comma-separated category names
            [FromQuery] string? searchTerm = null      // Optional filter: search by text in title/author/etc.
        )
        {
            try
            {
                // Calls the static method to retrieve articles from DB
                var savedArticles = SaveArticleRequest.GetSavedArticles(userId, page, pageSize, categories, searchTerm);
                return Ok(savedArticles); // Returns list as JSON
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Server error: {ex.Message}"); // Handles unexpected server errors
            }
        }

        // ===================== SAVE AN ARTICLE =====================
        // POST: api/savedarticle
        // Saves a new article to the user's saved list
        [HttpPost]
        public IActionResult SaveArticle([FromBody] SaveArticleRequest articleToSave)
        {
            try
            {
                // Calls instance method to insert into the DB
                int result = articleToSave.Save();

                if (result == 1)
                    return Ok(new { message = "Article saved successfully." }); // Success
                if (result == 0)
                    return Ok(new { message = "This article is already saved." }); // Duplicate entry
                return BadRequest(new { message = "Unknown error occurred." }); // Any other unexpected case
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }

        // ===================== DELETE A SAVED ARTICLE =====================
        // DELETE: api/savedarticle
        // Removes an article from the user's saved list
        [HttpDelete]
        public IActionResult DeleteArticle([FromBody] ArticleDeleteRequest request)
        {
            try
            {
                // Calls static method to remove from DB
                int result = SaveArticleRequest.Delete(request.UserId, request.ArticleUrl);

                if (result == 1)
                    return Ok(new { message = "Article removed from saved list." }); // Success
                return NotFound(new { message = "Article not found or not saved by user." }); // Nothing to delete
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }
    }
}
