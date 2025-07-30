using Microsoft.AspNetCore.Mvc;
using server.DAL;
using server.Models;
using System.Data.SqlClient;

namespace server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SharedArticleController : ControllerBase
    {
        // POST: api/sharedarticle
        // Shares an article with a user comment.
        [HttpPost]
        public IActionResult ShareArticle([FromBody] SharedArticleRequest shared)
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
        // GET: api/sharedarticle?hiddenUserIds=3,5,12&page=1&pageSize=10
        [HttpGet]
        public IActionResult GetSharedArticles([FromQuery] string? hiddenUserIds = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var list = SharedArticleIndex.GetAllSharedArticles(hiddenUserIds, page, pageSize);
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }


        // POST: api/sharedarticle/report
        // Reports a shared article by its ID and reason.
        [HttpPost("report")]
        public IActionResult ReportSharedArticle([FromBody] ReportSharedArticleRequest request)
        {
            try
            {
                int result = request.Report();

                return result switch
                {
                    1 => Ok(new { message = "Report submitted." }),
                    -1 => BadRequest(new { message = "You cannot report your own article." }),
                    -2 => NotFound(new { message = "Shared article not found." }),
                    _ => StatusCode(500, "Unknown result.")
                };
            }
            catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
            {
                return Conflict(new { message = "You already reported this article." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error.", error = ex.Message });
            }
        }

        // GET: api/sharedarticle/get-reported
        // Returns a list of shared article comments that have been reported.
        [HttpGet("get-reported")]
        public IActionResult GetReportedComments()
        {
            try
            {
                var dal = new DBservicesSharedArticles();
                var reportedList = dal.GetReportedComments();
                return Ok(reportedList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to load reported comments.", error = ex.Message });
            }
        }
    }
}
