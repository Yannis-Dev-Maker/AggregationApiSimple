using AgileActors.Models;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AgileActors.Services
{
    public class SpotifyService : IAggregatorService
    {
        public string Name => "spotify";
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public SpotifyService(IConfiguration configuration)
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
            var token = await GetSpotifyAccessTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                return new AggregatedResponse
                {
                    Results = new List<AggregatedResult>
                    {
                        new AggregatedResult
                        {
                            Source = "Spotify",
                            Title = $"Top track for {keyword}",
                            Description = "Unable to authenticate with Spotify",
                            Date = DateTime.Now,
                            Url = "https://spotify.com"
                        }
                    },
                    TotalCount = 0
                };
            }

            int offset = (page - 1) * count;
            int fetched = 0;
            int batchSize = 10;
            int totalCount = 0;
            var results = new List<AggregatedResult>();

            while (results.Count < count)
            {
                var batchUrl = $"https://api.spotify.com/v1/search?q={Uri.EscapeDataString(keyword.Trim())}&type=track&limit={batchSize}&offset={offset}";
                var request = new HttpRequestMessage(HttpMethod.Get, batchUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode) break;

                using var stream = await response.Content.ReadAsStreamAsync();
                using var json = await JsonDocument.ParseAsync(stream);

                var root = json.RootElement;
                var tracksElement = root.GetProperty("tracks");

                totalCount = tracksElement.GetProperty("total").GetInt32();
                var items = tracksElement.GetProperty("items").EnumerateArray().ToList();

                if (items.Count == 0) break;

                foreach (var track in items)
                {
                    try
                    {
                        string title = track.GetProperty("name").GetString() ?? "Unknown";
                        string artist = track.GetProperty("artists")[0].GetProperty("name").GetString() ?? "Unknown";
                        string urlToTrack = track.GetProperty("external_urls").GetProperty("spotify").GetString() ?? "https://spotify.com";

                        string? imageUrl = null;
                        if (track.TryGetProperty("album", out var albumProp) &&
                            albumProp.TryGetProperty("images", out var imagesProp) &&
                            imagesProp.ValueKind == JsonValueKind.Array &&
                            imagesProp.GetArrayLength() > 0)
                        {
                            imageUrl = imagesProp[0].GetProperty("url").GetString();
                        }

                        var result = new AggregatedResult
                        {
                            Source = "Spotify",
                            Title = $"Top track: {title}",
                            Description = $"Artist: {artist}",
                            Date = DateTime.Now,
                            Url = urlToTrack,
                            ImageUrl = imageUrl
                        };

                        results.Add(result);
                        if (results.Count >= count) break;
                    }
                    catch
                    {
                        continue;
                    }
                }

                if (items.Count < batchSize) break; // no more data
                offset += batchSize;
            }

            if (fromDate.HasValue)
                results = results.Where(r => r.Date >= fromDate.Value).ToList();

            return new AggregatedResponse
            {
                Results = results.Take(count).ToList(),
                TotalCount = totalCount
            };
        }

        private async Task<string> GetSpotifyAccessTokenAsync()
        {
            string clientId = _configuration["Spotify:ClientId"];
            string clientSecret = _configuration["Spotify:ClientSecret"];

            var credentials = $"{clientId}:{clientSecret}";
            var encodedCredentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials));

            var request = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token");
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", encodedCredentials);
            request.Content = new StringContent("grant_type=client_credentials", Encoding.UTF8, "application/x-www-form-urlencoded");

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode) return string.Empty;

            using var responseStream = await response.Content.ReadAsStreamAsync();
            using var json = await JsonDocument.ParseAsync(responseStream);

            return json.RootElement.GetProperty("access_token").GetString() ?? string.Empty;
        }
    }
}
