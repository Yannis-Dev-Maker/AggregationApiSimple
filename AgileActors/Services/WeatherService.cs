using AgileActors.Models;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text.Json;

namespace AgileActors.Services
{
    public class WeatherService : IAggregatorService
    {
        public string Name => "weather";
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public WeatherService(IConfiguration configuration)
        {
            _configuration = configuration;
            _httpClient = new HttpClient();
        }

        public async Task<AggregatedResponse> FetchDataAsync(
            string keyword,
            int count = 5,
            int page = 1,
            string sortBy = "publishedAt",
            DateTime? fromDate = null)
        {
            string apiKey = _configuration["OpenWeather:ApiKey"];
            string url = $"https://api.openweathermap.org/data/2.5/weather?q={keyword}&appid={apiKey}&units=metric";

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                return new AggregatedResponse
                {
                    Results = new List<AggregatedResult>
                    {
                        new AggregatedResult
                        {
                            Source = "OpenWeatherMap",
                            Title = $"Weather for {keyword}",
                            Description = $"Unable to retrieve weather data ({response.StatusCode})",
                            Date = DateTime.Now,
                            Url = "https://openweathermap.org"
                        }
                    },
                    TotalCount = 0
                };
            }

            using var responseStream = await response.Content.ReadAsStreamAsync();
            using var json = await JsonDocument.ParseAsync(responseStream);
            var root = json.RootElement;

            string description = root.GetProperty("weather")[0].GetProperty("description").GetString() ?? "No description";
            double temperature = root.GetProperty("main").GetProperty("temp").GetDouble();
            string iconCode = root.GetProperty("weather")[0].GetProperty("icon").GetString() ?? "01d";
            string imageUrl = $"https://openweathermap.org/img/wn/{iconCode}@2x.png";

            var result = new AggregatedResult
            {
                Source = "OpenWeatherMap",
                Title = $"Weather in {keyword}",
                Description = $"{description}, {temperature:0.00}°C",
                Date = DateTime.Now,
                Url = "https://openweathermap.org",
                ImageUrl = imageUrl
            };

            // Filter by fromDate (optional)
            if (fromDate.HasValue && result.Date < fromDate.Value)
            {
                return new AggregatedResponse
                {
                    Results = new List<AggregatedResult>(),
                    TotalCount = 0
                };
            }

            return new AggregatedResponse
            {
                Results = new List<AggregatedResult> { result },
                TotalCount = 1
            };
        }
    }
}
