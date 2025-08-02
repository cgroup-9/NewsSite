using Microsoft.AspNetCore.Mvc;
using server.DAL;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AiController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly string geminiApiKey;

        public AiController(IConfiguration config)
        {
            _httpClient = new HttpClient();
            geminiApiKey = config["ApiKeys:Gemini"];
        }

        [HttpPost("ask")]
        public async Task<IActionResult> AskAI([FromBody] string question)
        {
            if (string.IsNullOrWhiteSpace(question))
                return BadRequest("Question cannot be empty");

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = question }
                        }
                    }
                }
            };

            var jsonRequest = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(
                $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={geminiApiKey}",

                content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, error);
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(jsonResponse);
            string answer = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            return Ok(answer);
        }

        [HttpGet("question/{id}")]
        public async Task<IActionResult> GetAnswerByQuestionId(int id)
        {
            DBservicesAI db = new DBservicesAI();
            object data = null;
            string question = "";

            switch (id)
            {
                case 1: // Highest logins past week
                    var loginsWeek = db.GetLoginsRaw(7);
                    question = "Analyze the login data for the past week and determine which day had the highest number of logins. Here is the raw data: "
                               + JsonSerializer.Serialize(loginsWeek);
                    break;

                case 2: // Highest logins past month
                    var loginsMonth = db.GetLoginsRaw(30);
                    question = "Analyze the login data for the past month and determine which day had the highest number of logins. Here is the raw data: "
                               + JsonSerializer.Serialize(loginsMonth);
                    break;

                case 3: // Highest saved articles past month
                    var savedArticlesMonth = db.GetSavedArticlesRaw(30);
                    question = "Analyze the saved articles for the past month and determine which day had the highest number of saved articles. Here is the raw data: "
                               + JsonSerializer.Serialize(savedArticlesMonth);
                    break;

                case 4: // Category saved the most past month
                    var savedArticlesCategories = db.GetSavedArticlesRaw(30);
                    question = "Analyze the saved articles for the past month and determine which category was saved the most. Here is the raw data: "
                               + JsonSerializer.Serialize(savedArticlesCategories);
                    break;

                case 5: // Highest combined activity (logins + saves) past 6 months
                    var activityData = db.GetActivityRaw(180);
                    question = "Analyze the combined activity (logins + saved articles) for the past six months and determine which day had the highest combined activity. Here is the raw data: "
                               + JsonSerializer.Serialize(activityData);
                    break;


                default:
                    return BadRequest("Invalid question ID");
            }


            // שולחים את השאלה והדאטה ל-Gemini
            var requestBody = new
            {
                contents = new[]
                {
            new
            {
                parts = new[]
                {
                    new { text = question }
                }
            }
        }
            };

            var jsonRequest = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(
                $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={geminiApiKey}",
                content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, error);
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(jsonResponse);
            string answer = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            return Ok(new { answer = answer });
        }

    }
}
