using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Nest;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TheArchives.Server.Controllers;
using TheArchives.Server.Repositories;
using Xunit;

namespace TheArchives.Server.Tests.Unit.Controllers
{
    public class SearchControllerTests : UnitTestBase
    {
        private readonly SearchController Target;

        public SearchControllerTests() : base()
        {
            Target = new SearchController(
                The<ISearchRepository>().Object,
                The<ILogger<SearchController>>().Object,
                The<IMemoryCache>().Object,
                The<IMapper>().Object
            );
        }

        [Fact]
        public async Task GetSearch_ReturnsOk()
        {
            // Arrange
            var expectedSeed = 0xdead;
            var expectedPage = 2;
            var expectedPageSize = 9;
            var expectedTotal = 3;
            var expectedTook = 0xcafe;
            var expectedSearch = "EXPECTED SEARCH";
            var expectedRequest = new Shared.SearchRequest(expectedSeed, expectedPage, expectedPageSize, expectedSearch);
            var expectedDocs = Enumerable.Range(0, expectedTotal).Select(i =>
                new Models.Elastic.Content { ContentId = i }).ToArray();

            The<ISearchRepository>()
                .Setup(m => m.SearchAsync(expectedPage, expectedPageSize, expectedSearch, expectedSeed, It.IsAny<CancellationToken>()))
                .ReturnsAsync(The<ISearchResponse<Models.Elastic.Content>>().Object);

            The<ISearchResponse<Models.Elastic.Content>>().SetupGet(m => m.IsValid).Returns(true);
            The<ISearchResponse<Models.Elastic.Content>>().SetupGet(m => m.Documents).Returns(expectedDocs);
            The<ISearchResponse<Models.Elastic.Content>>().SetupGet(m => m.Total).Returns(expectedTotal);
            The<ISearchResponse<Models.Elastic.Content>>().SetupGet(m => m.Took).Returns(expectedTook);

            The<IMapper>()
                .Setup(m => m.Map<Shared.Content>(It.IsAny<Models.Elastic.Content>()))
                .Returns(new Shared.Content(0xdead, string.Empty, string.Empty, string.Empty, string.Empty, new string[0]));

            // Act
            var response = await Target.GetSearch(expectedRequest);

            // Assert
            Assert.Equal(expectedSearch, response.Value?.Search);
            Assert.Equal(expectedSeed, response.Value?.Seed);
            Assert.Equal(expectedPage, response.Value?.Page);
            Assert.Equal(expectedPageSize, response.Value?.PageSize);
            Assert.Equal(expectedTook, response.Value?.TimeTaken);
            Assert.Equal(expectedTotal, response.Value?.Total);
            Assert.Equal(expectedDocs.Select(d => 0xdead), response.Value?.Results.Select(r => r.ContentId));
            VerifyAll();
        }

        [Fact]
        public async Task GetSearch_Returns500()
        {
            // Arrange
            var expectedSeed = 0xdead;
            var expectedPage = 2;
            var expectedPageSize = 9;
            var expectedSearch = "EXPECTED SEARCH";
            var expectedRequest = new Shared.SearchRequest(expectedSeed, expectedPage, expectedPageSize, expectedSearch);

            The<ISearchRepository>()
                .Setup(m => m.SearchAsync(expectedPage, expectedPageSize, expectedSearch, expectedSeed, It.IsAny<CancellationToken>()))
                .ReturnsAsync(The<ISearchResponse<Models.Elastic.Content>>().Object);

            The<ISearchResponse<Models.Elastic.Content>>().SetupGet(m => m.IsValid).Returns(false);

            // Act
            var response = await Target.GetSearch(expectedRequest);

            // Assert
            Assert.IsType<ObjectResult>(response.Result);
            Assert.Equal(500, (response.Result as ObjectResult)?.StatusCode);
            VerifyAll();
        }

        [Fact]
        public async Task GetRelated_ReturnsOk()
        {
            // Arrange
            var expectedDocumentId = 0xdead;
            var expectedPage = 2;
            var expectedPageSize = 9;
            var expectedRequest = new Shared.RelatedRequest(expectedPage, expectedPageSize);
            var cacheKey = $"{nameof(Target.GetRelated)}_{expectedDocumentId}_{expectedPage}_{expectedPageSize}";
            var expectedTotal = 3;
            var expectedTook = 0xcafe;
            var expectedDocs = Enumerable.Range(0, expectedTotal).Select(i =>
                new Models.Elastic.Content { ContentId = i }).ToArray();

            The<ISearchRepository>()
                .Setup(m => m.MoreLikeThisAsync(expectedDocumentId, expectedPage, expectedPageSize, It.IsAny<CancellationToken>()))
                .ReturnsAsync(The<ISearchResponse<Models.Elastic.Content>>().Object);

            The<ISearchResponse<Models.Elastic.Content>>().SetupGet(m => m.IsValid).Returns(true);
            The<ISearchResponse<Models.Elastic.Content>>().SetupGet(m => m.Documents).Returns(expectedDocs);
            The<ISearchResponse<Models.Elastic.Content>>().SetupGet(m => m.Total).Returns(expectedTotal);
            The<ISearchResponse<Models.Elastic.Content>>().SetupGet(m => m.Took).Returns(expectedTook);

            object junk;
            The<IMemoryCache>().Setup(m => m.TryGetValue(cacheKey, out junk)).Returns(false);
            The<IMemoryCache>().Setup(m => m.CreateEntry(It.IsAny<string>())).Returns(The<ICacheEntry>().Object);

            The<IMapper>()
                .Setup(m => m.Map<Shared.Content>(It.IsAny<Models.Elastic.Content>()))
                .Returns(new Shared.Content(0xdead, string.Empty, string.Empty, string.Empty, string.Empty, new string[0]));

            // Act
            var response = await Target.GetRelated(expectedDocumentId, expectedRequest);

            // Assert
            Assert.Equal(expectedPage, response.Value?.Page);
            Assert.Equal(expectedPageSize, response.Value?.PageSize);
            Assert.Equal(expectedTook, response.Value?.TimeTaken);
            Assert.Equal(expectedTotal, response.Value?.Total);
            Assert.Equal(expectedDocs.Select(d => 0xdead), response.Value?.Results.Select(r => r.ContentId));
            VerifyAll();
        }

        [Fact]
        public async Task GetRelated_Returns500()
        {
            // Arrange
            var expectedDocumentId = 0xdead;
            var expectedPage = 2;
            var expectedPageSize = 9;
            var expectedRequest = new Shared.RelatedRequest(expectedPage, expectedPageSize);
            var cacheKey = $"{nameof(Target.GetRelated)}_{expectedDocumentId}_{expectedPage}_{expectedPageSize}";

            The<ISearchRepository>()
                .Setup(m => m.MoreLikeThisAsync(expectedDocumentId, expectedPage, expectedPageSize, It.IsAny<CancellationToken>()))
                .ReturnsAsync(The<ISearchResponse<Models.Elastic.Content>>().Object);

            The<ISearchResponse<Models.Elastic.Content>>().SetupGet(m => m.IsValid).Returns(false);

            object junk;
            The<IMemoryCache>().Setup(m => m.TryGetValue(cacheKey, out junk)).Returns(false);
            The<IMemoryCache>().Setup(m => m.CreateEntry(It.IsAny<string>())).Returns(The<ICacheEntry>().Object);

            // Act
            var response = await Target.GetRelated(expectedDocumentId, expectedRequest);

            // Assert
            Assert.IsType<ObjectResult>(response.Result);
            Assert.Equal(500, (response.Result as ObjectResult)?.StatusCode);
            VerifyAll();
        }
    }
}
