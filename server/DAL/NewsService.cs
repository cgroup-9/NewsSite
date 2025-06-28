using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using server.Models;

namespace server.DAL
{
    public class NewsService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public NewsService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "MyNewsApp");

            _apiKey = "bdbd65f8143048ee8f2dcc1592903813";
        }

        public async Task<List<Article>> GetTopHeadlinesAsync(string country = "us", string? categories = null)
        {
            // 🔄 Count this as a real API fetch (refresh/page load)
            DBservicesNews db = new DBservicesNews();
            db.IncrementApiFetchCounter();

            if (string.IsNullOrWhiteSpace(categories))
            {
                return await GetTopHeadlinesForCategoryAsync(country, null);
            }

            var categoriesList = categories.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                           .Select(c => c.ToLowerInvariant())
                                           .Distinct()
                                           .ToList();

            var allArticles = new List<Article>();

            foreach (var category in categoriesList)
            {
                var articlesForCategory = await GetTopHeadlinesForCategoryAsync(country, category);

                if (articlesForCategory != null)
                {
                    foreach (var article in articlesForCategory)
                        article.Category = category;

                    allArticles.AddRange(articlesForCategory);
                }
            }

            var distinctArticles = allArticles
                .GroupBy(a => a.Url)
                .Select(g => g.First())
                .ToList();

            return distinctArticles;
        }

        private async Task<List<Article>> GetTopHeadlinesForCategoryAsync(string country, string? category)
        {
            string url = $"https://newsapi.org/v2/top-headlines?country={country}&apiKey={_apiKey}";

            if (!string.IsNullOrEmpty(category))
                url += $"&category={category}";

            HttpResponseMessage response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                string message = $"News API failed ({(int)response.StatusCode} {response.StatusCode}): {errorContent}";
                throw new Exception(message);
            }

            string json = await response.Content.ReadAsStringAsync();

            NewsApiResponse? result = JsonSerializer.Deserialize<NewsApiResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return result?.Articles ?? new List<Article>();
        }

        public class NewsApiResponse
        {
            public string? Status { get; set; }
            public int TotalResults { get; set; }
            public List<Article>? Articles { get; set; }
        }
    }
}
