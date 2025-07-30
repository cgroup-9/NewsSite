using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using server.Models;
using server.DAL;

namespace server.DAL
{
    public class NewsService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public NewsService()
        {
            // Initialize HttpClient and set a User-Agent header (required by some APIs)
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "MyNewsApp");

            // API key for authenticating with the News API
            _apiKey = "bdbd65f8143048ee8f2dcc1592903813";
        }

        // This method fetches top news headlines from the API based on country and (optionally) multiple categories
        public async Task<List<Article>> GetTopHeadlinesAsync(string country = "us", string? categories = null)
        {
            // 🔄 Register this call as an API fetch (for statistics, logging, or limits)
            DBservicesArticles db = new DBservicesArticles();
            db.IncrementApiFetchCounter();

            // If no categories provided, fetch general headlines
            if (string.IsNullOrWhiteSpace(categories))
            {
                return await GetTopHeadlinesForCategoryAsync(country, null);
            }

            // Split the comma-separated list of categories into a cleaned-up distinct list
            var categoriesList = categories.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                           .Select(c => c.ToLowerInvariant())
                                           .Distinct()
                                           .ToList();

            var allArticles = new List<Article>();

            // Fetch articles for each category separately
            foreach (var category in categoriesList)
            {
                var articlesForCategory = await GetTopHeadlinesForCategoryAsync(country, category);

                if (articlesForCategory != null)
                {
                    // Mark each article with the corresponding category
                    foreach (var article in articlesForCategory)
                        article.Category = category;

                    allArticles.AddRange(articlesForCategory);
                }
            }

            // Remove duplicate articles (based on URL) to avoid showing the same article twice
            var distinctArticles = allArticles
                .GroupBy(a => a.Url)
                .Select(g => g.First())
                .ToList();

            return distinctArticles;
        }

        // This helper method fetches top headlines from the News API for a specific category and country
        private async Task<List<Article>> GetTopHeadlinesForCategoryAsync(string country, string? category)
        {
            // Construct the API URL with query parameters
            string url = $"https://newsapi.org/v2/top-headlines?country={country}&apiKey={_apiKey}";

            // Append category to the URL if it's specified
            if (!string.IsNullOrEmpty(category))
                url += $"&category={category}";

            // Make the HTTP request to the News API
            HttpResponseMessage response = await _httpClient.GetAsync(url);

            // If the response failed, throw an exception with detailed message
            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                string message = $"News API failed ({(int)response.StatusCode} {response.StatusCode}): {errorContent}";
                throw new Exception(message);
            }

            // Read the response body as JSON
            string json = await response.Content.ReadAsStringAsync();

            // Deserialize the JSON response into a NewsApiResponse object
            NewsApiResponse? result = JsonSerializer.Deserialize<NewsApiResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // Makes JSON parsing case-insensitive
            });

            // Return the list of articles, or an empty list if none were found
            return result?.Articles ?? new List<Article>();
        }

        // Internal class to match the structure of the News API JSON response
        public class NewsApiResponse
        {
            public string? Status { get; set; }           // "ok" or "error"
            public int TotalResults { get; set; }         // Total number of articles returned
            public List<Article>? Articles { get; set; }  // List of Article objects
        }
    }
}
