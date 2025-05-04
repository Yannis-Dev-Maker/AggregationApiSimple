using Xunit;
using AgileActors.Services;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace AgileActors.Tests.Services
{
    public class SpotifyServiceTests
    {
        private readonly ITestOutputHelper _output;

        public SpotifyServiceTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task FetchDataAsync_WithValidKeyword_ReturnsResults()
        {
            // Arrange
            var inMemorySettings = new Dictionary<string, string> {
                { "Spotify:ClientId", "49f812d86fe0418488efd6686f5b2191" },
                { "Spotify:ClientSecret", "a14531f015094ab180fd182b47fd4aad" }
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var service = new SpotifyService(configuration);

            // Act
            var result = await service.FetchDataAsync("Athens");

            // Log results
            _output.WriteLine($"Found {result.Results.Count} tracks for 'Athens':");
            foreach (var track in result.Results)
            {
                _output.WriteLine($"- {track.Title} by {track.Description}");
            }

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Results.Count > 0);
        }
    }
}
