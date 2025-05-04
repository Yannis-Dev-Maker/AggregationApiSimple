using Xunit;
using AgileActors.Services;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace AgileActors.Tests.Services
{
    public class NewsServiceTests
    {
        private readonly ITestOutputHelper _output;

        public NewsServiceTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task FetchDataAsync_WithValidKeyword_ReturnsResults()
        {
            // Arrange
            var inMemorySettings = new Dictionary<string, string> {
                { "NewsApi:ApiKey", "2076ee7428ed4a9686bfbdf716d1e769" }
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var service = new NewsService(configuration);

            // Act
            var result = await service.FetchDataAsync("Athens");

            // Log with test output helper
            _output.WriteLine($"Found {result.Results.Count} articles for 'Athens':");
            foreach (var article in result.Results)
            {
                _output.WriteLine($"- {article.Title} ({article.Date:yyyy-MM-dd})");
            }

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Results.Count > 0);
        }
    }
}
