using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using server.Models;

namespace server.DAL
{
    public class NewsService
    {
        private readonly HttpClient _httpClient;  //object for HTTP calls like GET, POST from web
        private readonly string _apiKey; 

        public NewsService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "MyNewsApp"); // for non annonumus call

            _apiKey = "bdbd65f8143048ee8f2dcc1592903813";
        }

        public async Task<List<Article>> GetTopHeadlinesAsync(string country = "us")
        {
            string url = $"https://newsapi.org/v2/top-headlines?country={country}&apiKey={_apiKey}";

            HttpResponseMessage response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();

                NewsApiResponse? result = JsonSerializer.Deserialize<NewsApiResponse>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result?.Articles ?? new List<Article>();
            }
            else
            {
                string errorContent = await response.Content.ReadAsStringAsync(); // ניתוח שגיאה
                string message = $"News API failed ({(int)response.StatusCode} {response.StatusCode}): {errorContent}";
                throw new Exception(message);
            }
        }
    }

    public class NewsApiResponse
    {
        public string? Status { get; set; }
        public int TotalResults { get; set; }
        public List<Article>? Articles { get; set; }
    }
}
