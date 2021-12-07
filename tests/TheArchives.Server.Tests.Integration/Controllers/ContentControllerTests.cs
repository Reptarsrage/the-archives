using System.Collections.Generic;
using System.IO;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

using Moq;

using TheArchives.Server.Repositories;

using Xunit;

namespace TheArchives.Server.Tests.Integration.Controllers
{
    public class ContentControllerTests : IntegrationTestBase
    {
        private const string BaseDir = "c:/testing/";

        public ContentControllerTests(CustomWebApplicationFactory factory) : base(factory)
        {
        }

        protected override IDictionary<string, string> ConfigureConfiguration()
        {
            // Override this to inject any necessary test-specific configuration variables
            return new Dictionary<string, string>
            {
                ["Content:BaseDir"] = BaseDir,
            };
        }

        [Fact]
        public async Task GetContentTextById_ReturnsOK()
        {
            // Arrange
            var expectedContentId = 0xdead;
            var expectedUrl = $"/api/content/text/{expectedContentId}";
            var expectedPath = "fake/path/document.txt";
            var expectedText = "EXPECTED TEXT";
            var expectedContentDto = new Models.Dto.Content { Path = expectedPath };

            The<IContentRepository>()
                .Setup(m => m.GetAsync(expectedContentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedContentDto);

            The<ITextRepository>()
                .Setup(m => m.ReadAsync(Path.Combine(BaseDir, expectedPath), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedText);

            // Act
            var actualText = await _client.GetStringAsync(expectedUrl);

            // Assert
            Assert.Equal(expectedText, actualText);
            VerifyAll();
        }

        [Fact]
        public async Task GetContentById_ReturnsOK()
        {
            // Arrange
            var expectedContentId = 0xdead;
            var expectedUrl = $"/api/content/{expectedContentId}";
            var expectedTitle = "EXPECTED TITLE";
            var expectedContentDto = new Models.Dto.Content { Title = expectedTitle };

            The<IContentRepository>()
                .Setup(m => m.GetAsync(expectedContentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedContentDto);

            // Act
            var actualResponse = await _client.GetFromJsonAsync<Shared.Content>(expectedUrl);

            // Assert
            Assert.Equal(expectedTitle, actualResponse?.Title);
            VerifyAll();
        }
    }
}