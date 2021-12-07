using System.IO;
using System.Threading;
using System.Threading.Tasks;

using AutoMapper;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Moq;

using TheArchives.Server.Controllers;
using TheArchives.Server.Repositories;

using Xunit;

namespace TheArchives.Server.Tests.Unit.Controllers
{
    public class ContentControllerTests : UnitTestBase
    {
        private readonly ContentController Target;

        public ContentControllerTests() : base()
        {
            Target = new ContentController(
                The<ITextRepository>().Object,
                The<ILogger<ContentController>>().Object,
                The<IMemoryCache>().Object,
                The<IMapper>().Object,
                The<IContentRepository>().Object,
                The<IOptions<Models.ContentOptions>>().Object
            );
        }

        [Fact]
        public async Task GetContentTextById_ReturnsOk()
        {
            // Arrange
            var expectedContentId = 0xdead;
            var expectedPath = "fake/path/document.txt";
            var expectedBaseDir = "c:/testing/";
            var expectedOptions = new Models.ContentOptions { BaseDir = expectedBaseDir };
            var expectedContentDto = new Models.Dto.Content { Path = expectedPath };
            var expectedFullPath = Path.Combine(expectedBaseDir, expectedPath);
            var expectedText = "EXPECTED TEXT";
            var cacheKey1 = $"{nameof(ContentController)}_{expectedContentId}";
            var cacheKey2 = $"{nameof(ContentController)}_{expectedFullPath}";

            object junk;
            The<IMemoryCache>().Setup(m => m.TryGetValue(cacheKey1, out junk)).Returns(false);
            The<IMemoryCache>().Setup(m => m.TryGetValue(cacheKey2, out junk)).Returns(false);
            The<IMemoryCache>().Setup(m => m.CreateEntry(It.IsAny<string>())).Returns(The<ICacheEntry>().Object);

            The<IOptions<Models.ContentOptions>>().SetupGet(mbox => mbox.Value).Returns(expectedOptions);

            The<IContentRepository>()
                .Setup(m => m.GetAsync(expectedContentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedContentDto);

            The<ITextRepository>()
                .Setup(m => m.ReadAsync(expectedFullPath, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedText);

            // Act
            var response = await Target.GetContentTextById(expectedContentId);

            // Assert
            Assert.Equal(expectedText, response.Value);
            VerifyAll();
        }

        [Fact]
        public async Task GetContentTextById_ReturnsNotFound()
        {
            // Arrange
            var expectedContentId = 0xdead;
            Models.Dto.Content? expectedContentDto = null;
            var cacheKey1 = $"{nameof(ContentController)}_{expectedContentId}";

            object junk;
            The<IMemoryCache>().Setup(m => m.TryGetValue(cacheKey1, out junk)).Returns(false);
            The<IMemoryCache>().Setup(m => m.CreateEntry(It.IsAny<string>())).Returns(The<ICacheEntry>().Object);

            The<IContentRepository>()
                .Setup(mbox => mbox.GetAsync(expectedContentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedContentDto);

            // Act
            var response = await Target.GetContentTextById(expectedContentId);

            // Assert
            Assert.IsType<NotFoundResult>(response.Result);
            VerifyAll();
        }

        [Fact]
        public async Task GetContentById_ReturnsOk()
        {
            // Arrange
            var expectedContentId = 0xdead;
            var expectedTitle = "EXPECTED TITLE";
            var expectedContentDto = new Models.Dto.Content { Title = expectedTitle };
            var cacheKey1 = $"{nameof(ContentController)}_{expectedContentId}";
            var expectedModel = new Shared.Content(expectedContentId, string.Empty, string.Empty, string.Empty, string.Empty, new string[0]);

            object junk;
            The<IMemoryCache>().Setup(m => m.TryGetValue(cacheKey1, out junk)).Returns(false);
            The<IMemoryCache>().Setup(m => m.CreateEntry(It.IsAny<string>())).Returns(The<ICacheEntry>().Object);

            The<IContentRepository>()
                .Setup(m => m.GetAsync(expectedContentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedContentDto);

            The<IMapper>()
                .Setup(m => m.Map<Shared.Content>(expectedContentDto))
                .Returns(expectedModel);

            // Act
            var response = await Target.GetContentById(expectedContentId);

            // Assert
            Assert.Same(expectedModel, response.Value);
            VerifyAll();
        }

        [Fact]
        public async Task GetContentById_ReturnsNotFound()
        {
            // Arrange
            var expectedContentId = 0xdead;
            Models.Dto.Content? expectedContentDto = null;
            var cacheKey1 = $"{nameof(ContentController)}_{expectedContentId}";

            object junk;
            The<IMemoryCache>().Setup(m => m.TryGetValue(cacheKey1, out junk)).Returns(false);
            The<IMemoryCache>().Setup(m => m.CreateEntry(It.IsAny<string>())).Returns(The<ICacheEntry>().Object);

            The<IContentRepository>()
                .Setup(m => m.GetAsync(expectedContentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedContentDto);

            // Act
            var response = await Target.GetContentById(expectedContentId);

            // Assert
            Assert.IsType<NotFoundResult>(response.Result);
            VerifyAll();
        }
    }
}