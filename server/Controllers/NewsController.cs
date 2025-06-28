using Microsoft.AspNetCore.Mvc;
using server.DAL;
using server.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewsController : ControllerBase
    {
        private readonly NewsService news;
        public NewsController()
        {
            news = new NewsService();
        }

        // GET api/news?country=il
        [HttpGet]
        public async Task<IActionResult> GetNews([FromQuery] string country = "us", [FromQuery] string? categories = null)
        {
            try
            {
                List<Article> articles = await news.GetTopHeadlinesAsync(country, categories);

                if (!string.IsNullOrEmpty(categories))
                {
                    var categoriesList = categories.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                                   .Select(c => c.ToLowerInvariant())
                                                   .ToList();

                    articles = articles.Where(article =>
                        article.Category == null || 
                        categoriesList.Contains(article.Category.ToLowerInvariant()) 
                    ).ToList();
                }
                return Ok(articles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET api/<NewsController>/5
        [HttpGet("Saved/{userId}")]
        public IActionResult Get(int userId)
        {
            DBservicesArticles db = new DBservicesArticles();
            try
            {
                List<SavedArticle> result = db.GetSavedArticles(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }

        // POST api/<NewsController>
        [HttpPost("Save-Article")]
        public IActionResult Post([FromBody] SavedArticle articleToSave)
        {
            DBservicesArticles db = new DBservicesArticles();
            int result = db.saveArticle(articleToSave);

            if (result == 1)
                return Ok(new { message = "Article saved successfully." });
            if (result == 0)
                return Ok(new { message = "This article is already saved." });
            return BadRequest(new { message = "Unknown error occurred." });

        }

        // PUT api/<NewsController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<NewsController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
