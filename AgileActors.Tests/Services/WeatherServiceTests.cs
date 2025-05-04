using Xunit;
using AgileActors.Services;
using Microsoft.Extensions.Configuration;
using Xunit.Abstractions;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace AgileActors.Tests.Services
{
    public class WeatherServiceTests
    {
        private readonly ITestOutputHelper _output;

        public WeatherServiceTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task FetchDataAsync_WithValidCity_ReturnsWeather()
        {
            // Arrange
            var inMemorySettings = new Dictionary<string, string> {
                { "OpenWeather:ApiKey", "d3481a9c28855540dee30d36d197a79d" }
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var service = new WeatherService(configuration);

            // Act
            var result = await service.FetchDataAsync("Athens");

            // Log output
            _output.WriteLine($"Received {result.Results.Count} weather result(s):");
            foreach (var item in result.Results)
            {
                _output.WriteLine($"- {item.Title}: {item.Description}");
            }

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Results.Count > 0);
        }
    }
}
