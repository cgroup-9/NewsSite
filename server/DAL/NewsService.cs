using System.Net.Http;     // For making HTTP requests
using System.Text.Json;    // For serializing and deserializing JSON
using System.Threading.Tasks;
using System.Collections.Generic;
using server.Models;       // Contains the Article model
using server.DAL;          // Data Access Layer for DB services

namespace server.DAL
{
    public class NewsService
    {
        private readonly HttpClient _httpClient; // Used for making requests to the News API
        private readonly string _apiKey;         // News API key (loaded from configuration)

        // Constructor – receives configuration so it can read the API key
        public NewsService(IConfiguration config)
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "MyNewsApp");
            // Some APIs require a User-Agent header, so we set a default

            _apiKey = config["ApiKeys:NewsApi"];
            // Read the News API key from appsettings.json or environment variables
        }

        // ===================== MAIN METHOD =====================
        // Fetches top headlines from News API for a specific country
        // Optionally supports multiple categories separated by commas
        public async Task<List<Article>> GetTopHeadlinesAsync(string country = "us", string? categories = null)
        {
            // 1️⃣ Log this API request in the database for analytics/statistics
            DBservicesArticles db = new DBservicesArticles();
            db.IncrementApiFetchCounter();

            // 2️⃣ If no categories provided → fetch general top headlines
            if (string.IsNullOrWhiteSpace(categories))
            {
                return await GetTopHeadlinesForCategoryAsync(country, null);
            }

            // 3️⃣ Parse the comma-separated categories into a clean list (lowercase, no duplicates)
            var categoriesList = categories
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(c => c.ToLowerInvariant())
                .Distinct()
                .ToList();

            var allArticles = new List<Article>();

            // 4️⃣ Loop over each category and fetch its headlines separately
            foreach (var category in categoriesList)
            {
                var articlesForCategory = await GetTopHeadlinesForCategoryAsync(country, category);

                if (articlesForCategory != null)
                {
                    // Tag each article with its category
                    foreach (var article in articlesForCategory)
                        article.Category = category;

                    allArticles.AddRange(articlesForCategory);
                }
            }

            // 5️⃣ Remove duplicates based on URL (avoids showing the same article twice)
            var distinctArticles = allArticles
                .GroupBy(a => a.Url)
                .Select(g => g.First())
                .ToList();

            return distinctArticles;
        }

        // ===================== HELPER METHOD =====================
        // Fetches top headlines for a single category and country
        private async Task<List<Article>> GetTopHeadlinesForCategoryAsync(string country, string? category)
        {
            // 1️⃣ Build the base API URL with country and API key
            string url = $"https://newsapi.org/v2/top-headlines?country={country}&apiKey={_apiKey}";

            // 2️⃣ If category is specified, add it to the URL
            if (!string.IsNullOrEmpty(category))
                url += $"&category={category}";

            // 3️⃣ Send GET request to News API
            HttpResponseMessage response = await _httpClient.GetAsync(url);

            // 4️⃣ If request failed, read the error body and throw an exception
            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                string message = $"News API failed ({(int)response.StatusCode} {response.StatusCode}): {errorContent}";
                throw new Exception(message);
            }

            // 5️⃣ Read the JSON response as string
            string json = await response.Content.ReadAsStringAsync();

            // 6️⃣ Deserialize JSON into a NewsApiResponse object
            NewsApiResponse? result = JsonSerializer.Deserialize<NewsApiResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // Ignore case in JSON property names
            });

            // 7️⃣ Return articles list, or empty list if null
            return result?.Articles ?? new List<Article>();
        }

        // ===================== RESPONSE MODEL =====================
        // Matches the structure of the News API's JSON response
        public class NewsApiResponse
        {
            public string? Status { get; set; }           // "ok" or "error"
            public int TotalResults { get; set; }         // Total number of results
            public List<Article>? Articles { get; set; }  // List of articles
        }
    }
}
