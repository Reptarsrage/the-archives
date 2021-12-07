using System.Linq;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

using Moq;

using TheArchives.Server.Repositories;

using Xunit;

namespace TheArchives.Server.Tests.Integration.Controllers
{
    public class SearchControllerTests : IntegrationTestBase
    {
        public SearchControllerTests(CustomWebApplicationFactory factory) : base(factory)
        {
        }

        [Fact]
        public async Task GetSearch_ReturnsOK()
        {
            // Arrange
            var expectedUrl = "/api/search";
            var expectedTotal = 10;
            var expectedTook = 0xdead;
            var expectedPage = 0;
            var expectedPageSize = 20;
            var expectedSearch = string.Empty;
            var expectedDocs = Enumerable.Range(0, expectedTotal).Select(i => new Models.Elastic.Content
            {
                ContentId = i,
                Title = $"CONTENT #{i}"
            }).ToArray();

            The<Nest.ISearchResponse<Models.Elastic.Content>>().SetupGet(m => m.IsValid).Returns(true);
            The<Nest.ISearchResponse<Models.Elastic.Content>>().SetupGet(m => m.Total).Returns(expectedTotal);
            The<Nest.ISearchResponse<Models.Elastic.Content>>().SetupGet(m => m.Took).Returns(expectedTook);
            The<Nest.ISearchResponse<Models.Elastic.Content>>().SetupGet(m => m.Documents).Returns(expectedDocs);

            The<ISearchRepository>()
                .Setup(m => m.SearchAsync(expectedPage, expectedPageSize, expectedSearch, It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(The<Nest.ISearchResponse<Models.Elastic.Content>>().Object);

            // Act
            var actualResponse = await _client.GetFromJsonAsync<Shared.SearchResponse>(expectedUrl);

            // Assert
            Assert.Equal(expectedTotal, actualResponse?.Total);
            Assert.Equal(expectedTook, actualResponse?.TimeTaken);
            Assert.Equal(expectedPage, actualResponse?.Page);
            Assert.Equal(expectedPageSize, actualResponse?.PageSize);
            Assert.Equal(expectedSearch, actualResponse?.Search);
            Assert.Equal(expectedDocs.Select(d => d.Title), actualResponse?.Results.Select(r => r.Title));
            VerifyAll();
        }

        [Fact]
        public async Task GetRelated_ReturnsOK()
        {
            // Arrange
            var expectedContentId = 0xbeef;
            var expectedUrl = $"/api/search/{expectedContentId}";
            var expectedTotal = 10;
            var expectedTook = 0xdead;
            var expectedPage = 0;
            var expectedPageSize = 20;
            var expectedDocs = Enumerable.Range(0, expectedTotal).Select(i => new Models.Elastic.Content
            {
                ContentId = i,
                Title = $"CONTENT #{i}"
            }).ToArray();

            The<Nest.ISearchResponse<Models.Elastic.Content>>().SetupGet(m => m.IsValid).Returns(true);
            The<Nest.ISearchResponse<Models.Elastic.Content>>().SetupGet(m => m.Total).Returns(expectedTotal);
            The<Nest.ISearchResponse<Models.Elastic.Content>>().SetupGet(m => m.Took).Returns(expectedTook);
            The<Nest.ISearchResponse<Models.Elastic.Content>>().SetupGet(m => m.Documents).Returns(expectedDocs);

            The<ISearchRepository>()
                .Setup(m => m.MoreLikeThisAsync(expectedContentId, expectedPage, expectedPageSize, It.IsAny<CancellationToken>()))
                .ReturnsAsync(The<Nest.ISearchResponse<Models.Elastic.Content>>().Object);

            // Act
            var actualResponse = await _client.GetFromJsonAsync<Shared.RelatedResponse>(expectedUrl);

            // Assert
            Assert.Equal(expectedTotal, actualResponse?.Total);
            Assert.Equal(expectedTook, actualResponse?.TimeTaken);
            Assert.Equal(expectedPage, actualResponse?.Page);
            Assert.Equal(expectedPageSize, actualResponse?.PageSize);
            Assert.Equal(expectedDocs.Select(d => d.Title), actualResponse?.Results.Select(r => r.Title));
            VerifyAll();
        }
    }
}