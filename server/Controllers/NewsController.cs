using Microsoft.AspNetCore.Mvc;
using server.DAL;
using server.Models;

namespace server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArticlesController : ControllerBase
    {
        private readonly NewsService news;

        public ArticlesController()
        {
            news = new NewsService();
        }

        // GET api/articles?country=il&categories=technology,health
        [HttpGet]
        public async Task<IActionResult> GetNews([FromQuery] string country = "us", [FromQuery] string? categories = null)
        {
            try
            {
                List<Article> articles = await news.GetTopHeadlinesAsync(country, categories);

                return Ok(articles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
