using Microsoft.AspNetCore.Mvc;  // Provides attributes and base classes for building APIs
using server.DAL;                // Data Access Layer for database operations
using server.Models;             // Models used for data transfer and business logic
using System.Data.SqlClient;     // For handling SQL-specific exceptions

namespace server.Controllers
{
    [Route("api/[controller]")] // Base route → "api/SharedArticle"
    [ApiController] // Marks this as an API controller (auto model binding, validation, etc.)
    public class SharedArticleController : ControllerBase
    {
        // ===================== SHARE AN ARTICLE =====================
        // POST: api/sharedarticle
        // Shares an article along with a user comment
        [HttpPost]
        public IActionResult ShareArticle([FromBody] SharedArticleRequest shared)
        {
            try
            {
                int result = shared.Share(); // Calls the Share() method in the model to insert into DB
                if (result == 1)
                    return Ok(new { message = "Article shared successfully." }); // Success
                return BadRequest(new { message = "Failed to share article." }); // Failure
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Server error: {ex.Message}"); // Unhandled error
            }
        }

        // ===================== GET SHARED ARTICLES =====================
        // GET: api/sharedarticle?hiddenUserIds=3,5,12&page=1&pageSize=10&userId=7
        // Returns a paginated list of shared articles, optionally filtering out hidden users
        [HttpGet]
        public IActionResult GetSharedArticles(
            [FromQuery] string? hiddenUserIds = null, // Comma-separated list of user IDs to exclude
            [FromQuery] int page = 1,                // Page number (default: 1)
            [FromQuery] int pageSize = 20,           // Items per page (default: 20)
            [FromQuery] int? userId = null           // Optional: filter by a specific user ID
        )
        {
            try
            {
                var list = SharedArticleIndex.GetAllSharedArticles(hiddenUserIds, page, pageSize, userId);
                return Ok(list); // Returns the list as JSON
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }

        // ===================== REPORT A SHARED ARTICLE =====================
        // POST: api/sharedarticle/report
        // Reports a shared article with a reason
        [HttpPost("report")]
        public IActionResult ReportSharedArticle([FromBody] ReportSharedArticleRequest request)
        {
            try
            {
                int result = request.Report(); // Calls model method to insert a report into DB

                return result switch
                {
                    1 => Ok(new { message = "Report submitted." }),              // Success
                    -1 => BadRequest(new { message = "You cannot report your own article." }),
                    -2 => NotFound(new { message = "Shared article not found." }), // Invalid ID
                    _ => StatusCode(500, "Unknown result.")                       // Unexpected return
                };
            }
            catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627) // SQL duplicate key
            {
                return Conflict(new { message = "You already reported this article." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error.", error = ex.Message });
            }
        }

        // ===================== GET REPORTED COMMENTS (ADMIN) =====================
        // GET: api/sharedarticle/get-reported
        // Returns a list of comments on shared articles that have been reported
        [HttpGet("get-reported")]
        public IActionResult GetReportedComments()
        {
            try
            {
                var dal = new DBservicesSharedArticles(); // DAL instance for DB queries
                var reportedList = dal.GetReportedComments(); // Retrieves all reported comments
                return Ok(reportedList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to load reported comments.", error = ex.Message });
            }
        }

        // ===================== LIKE A SHARED ARTICLE =====================
        // POST: api/sharedarticle/like
        // Allows a user to like a shared article
        [HttpPost("like")]
        public async Task<IActionResult> LikeArticle([FromBody] LikeRequest like)
        {
            try
            {
                var db = new DBservicesSharedArticles();
                bool success = await db.LikeSharedArticleAsync(like.SharedArticleId, like.UserId); // Adds like in DB
                return Ok(new { message = "Liked successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message); // Likely invalid input or DB error
            }
        }

        // ===================== UNLIKE A SHARED ARTICLE =====================
        // POST: api/sharedarticle/unlike
        // Allows a user to remove a like from a shared article
        [HttpPost("unlike")]
        public async Task<IActionResult> UnlikeArticle([FromBody] LikeRequest like)
        {
            try
            {
                var db = new DBservicesSharedArticles();
                bool success = await db.UnlikeSharedArticleAsync(like.SharedArticleId, like.UserId); // Removes like in DB
                return Ok(new { message = "Unliked successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
