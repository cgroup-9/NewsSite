using Microsoft.AspNetCore.Mvc; // API controller functionality
using server.DAL;               // Data Access Layer for fetching raw data from DB
using System.Net.Http;          // HttpClient for sending HTTP requests
using System.Text;              // For Encoding.UTF8
using System.Text.Json;         // For serializing/deserializing JSON
using System.Threading.Tasks;   // For async/await

namespace server.Controllers
{
    [Route("api/[controller]")] // Base URL: api/ai
    [ApiController]             // Enables automatic model binding and validation
    public class AiController : ControllerBase
    {
        private readonly HttpClient _httpClient; // Used to send requests to the Gemini API
        private readonly string geminiApiKey;    // API key from configuration

        // Constructor – injects configuration to read API key
        public AiController(IConfiguration config)
        {
            _httpClient = new HttpClient();
            geminiApiKey = config["ApiKeys:Gemini"]; // Read Gemini API key from appsettings.json
        }

        // ===================== ASK AI DIRECTLY =====================
        // POST: api/ai/ask
        // Sends a user question directly to the Gemini AI API and returns the AI's answer
        [HttpPost("ask")]
        public async Task<IActionResult> AskAI([FromBody] string question)
        {
            if (string.IsNullOrWhiteSpace(question))
                return BadRequest("Question cannot be empty"); // Validate input

            // Build request body for Gemini API
            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = question } // The question text to send to AI
                        }
                    }
                }
            };

            // Serialize to JSON
            var jsonRequest = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            // Send POST request to Gemini API endpoint
            var response = await _httpClient.PostAsync(
                $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={geminiApiKey}",
                content);

            // If API call fails – return status code with error message
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, error);
            }

            // Parse JSON response
            var jsonResponse = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(jsonResponse);
            string answer = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            // Return AI's answer
            return Ok(answer);
        }

        // ===================== ASK AI BASED ON PREDEFINED QUESTIONS =====================
        // GET: api/ai/question/{id}
        // Uses predefined question IDs to send data from DB to AI and analyze it
        [HttpGet("question/{id}")]
        public async Task<IActionResult> GetAnswerByQuestionId(int id)
        {
            DBservicesAI db = new DBservicesAI(); // DB service for retrieving raw data
            string question = "";

            switch (id)
            {
                case 1: // Highest logins in the past week
                    var loginsWeek = db.GetLoginsRaw(7);
                    question = "Analyze the login data for the past week and determine which day had the highest number of logins. Here is the raw data: "
                               + JsonSerializer.Serialize(loginsWeek);
                    break;

                case 2: // Highest logins in the past month
                    var loginsMonth = db.GetLoginsRaw(30);
                    question = "Analyze the login data for the past month and determine which day had the highest number of logins. Here is the raw data: "
                               + JsonSerializer.Serialize(loginsMonth);
                    break;

                case 3: // Highest saved articles in the past month
                    var savedArticlesMonth = db.GetSavedArticlesRaw(30);
                    question = "Analyze the saved articles for the past month and determine which day had the highest number of saved articles. Here is the raw data: "
                               + JsonSerializer.Serialize(savedArticlesMonth);
                    break;

                case 4: // Most saved category in the past month
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
                    return BadRequest("Invalid question ID"); // If ID is not recognized
            }

            // Build request body for Gemini with the generated question
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

            // Serialize request
            var jsonRequest = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            // Send POST request to Gemini
            var response = await _httpClient.PostAsync(
                $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={geminiApiKey}",
                content);

            // Handle failure from Gemini API
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, error);
            }

            // Parse response and extract answer text
            var jsonResponse = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(jsonResponse);
            string answer = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            // Return as JSON object
            return Ok(new { answer = answer });
        }
    }
}
