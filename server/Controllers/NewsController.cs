using Microsoft.AspNetCore.Mvc; // Provides attributes and base classes for building APIs
using server.DAL;               // Data Access Layer – contains NewsService for fetching external API data
using server.Models;            // Contains the Article model

namespace server.Controllers
{
    [Route("api/[controller]")] // Base route → "api/Articles"
    [ApiController]             // Enables automatic model binding, validation, etc.
    public class ArticlesController : ControllerBase
    {
        private readonly NewsService news; // Service class for fetching news articles from an external API

        // Constructor – receives IConfiguration for reading settings (like API keys) from appsettings.json
        public ArticlesController(IConfiguration config)
        {
            news = new NewsService(config); // Initialize the NewsService with configuration
        }

        // ===================== GET NEWS ARTICLES =====================
        // GET: api/articles?country=il&categories=technology,health
        // Retrieves top headlines based on country and optional category filter
        [HttpGet]
        public async Task<IActionResult> GetNews(
            [FromQuery] string country = "us",        // Country code (default: "us")
            [FromQuery] string? categories = null    // Optional: comma-separated category list
        )
        {
            try
            {
                // Calls NewsService to fetch the news from an external API
                List<Article> articles = await news.GetTopHeadlinesAsync(country, categories);

                // Returns the list of articles as JSON (HTTP 200 OK)
                return Ok(articles);
            }
            catch (Exception ex)
            {
                // Handles any unexpected errors – returns HTTP 500
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
