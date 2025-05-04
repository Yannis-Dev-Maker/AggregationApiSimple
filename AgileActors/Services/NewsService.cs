using AgileActors.Models;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text.Json;

namespace AgileActors.Services
{
    public class NewsService : IAggregatorService

    {
        public string Name => "news";
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public NewsService(IConfiguration configuration)
        {
            _configuration = configuration;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("AgileAggregatorDemo/1.0");
        }

        public async Task<AggregatedResponse> FetchDataAsync(
            string keyword,
            int count = 5,
            int page = 1,
            string sortBy = "publishedAt",
            DateTime? fromDate = null)
        {
            string apiKey = _configuration["NewsApi:ApiKey"];
            string fromDateStr = (fromDate ?? new DateTime(2025, 4, 1)).ToString("yyyy-MM-dd");
            string sort = string.IsNullOrWhiteSpace(sortBy) ? "publishedAt" : sortBy.ToLower();

            if (sort != "publishedat" && sort != "relevancy" && sort != "popularity")
                sort = "publishedAt";

            string url = $"https://newsapi.org/v2/everything?q={Uri.EscapeDataString(keyword)}&from={fromDateStr}&sortBy={sort}&language=en&pageSize={count}&page={page}&apiKey={apiKey}";

            try
            {
                var response = await _httpClient.GetAsync(url);
                using var stream = await response.Content.ReadAsStreamAsync();
                using var json = await JsonDocument.ParseAsync(stream);
                var root = json.RootElement;

                string status = root.GetProperty("status").GetString();
                if (status != "ok")
                {
                    string message = root.TryGetProperty("message", out var msgProp)
                        ? msgProp.GetString()
                        : "Unknown NewsAPI error";

                    return new AggregatedResponse
                    {
                        Results = new List<AggregatedResult>
                        {
                            new AggregatedResult
                            {
                                Source = "NewsAPI",
                                Title = $"News for {keyword}",
                                Description = $"NewsAPI error: {message}",
                                Date = DateTime.Now,
                                Url = "https://newsapi.org"
                            }
                        },
                        TotalCount = 0
                    };
                }

                var articles = root.GetProperty("articles").EnumerateArray().ToList();
                int totalResults = root.TryGetProperty("totalResults", out var totalProp) ? totalProp.GetInt32() : 0;

                if (articles.Count == 0)
                {
                    return new AggregatedResponse
                    {
                        Results = new List<AggregatedResult>
                        {
                            new AggregatedResult
                            {
                                Source = "NewsAPI",
                                Title = $"No news for {keyword}",
                                Description = "No articles found",
                                Date = DateTime.Now,
                                Url = "https://newsapi.org"
                            }
                        },
                        TotalCount = 0
                    };
                }

                var results = articles.Select(article =>
                {
                    string title = article.TryGetProperty("title", out var titleProp) ? titleProp.GetString() ?? "Untitled" : "Untitled";
                    string description = article.TryGetProperty("description", out var descProp) ? descProp.GetString() ?? "No description" : "No description";
                    string urlToArticle = article.TryGetProperty("url", out var urlProp) ? urlProp.GetString() ?? "https://newsapi.org" : "https://newsapi.org";
                    string imageUrl = article.TryGetProperty("urlToImage", out var imgProp) ? imgProp.GetString() : null;
                    DateTime publishedAt = article.TryGetProperty("publishedAt", out var dateProp) &&
                                           dateProp.ValueKind == JsonValueKind.String &&
                                           DateTime.TryParse(dateProp.GetString(), out var parsedDate)
                        ? parsedDate
                        : DateTime.UtcNow;

                    return new AggregatedResult
                    {
                        Source = "NewsAPI",
                        Title = title,
                        Description = description,
                        Date = publishedAt,
                        Url = urlToArticle,
                        ImageUrl = imageUrl
                    };
                }).ToList();

                return new AggregatedResponse
                {
                    Results = results,
                    TotalCount = totalResults
                };
            }
            catch (Exception ex)
            {
                return new AggregatedResponse
                {
                    Results = new List<AggregatedResult>
                    {
                        new AggregatedResult
                        {
                            Source = "NewsAPI",
                            Title = $"Error fetching news for {keyword}",
                            Description = ex.Message,
                            Date = DateTime.Now,
                            Url = "https://newsapi.org"
                        }
                    },
                    TotalCount = 0
                };
            }
        }
    }
}
