using Bunit;
using Bunit.TestDoubles;

using TheArchives.Client.Shared;

using Xunit;

namespace TheArchives.Client.Tests.Unit.Shared
{
    // see: https://bunit.dev/docs/getting-started/index.html
    public class LoaderTests
    {
        [Fact]
        public void LoaderShouldRender_WhenLoading()
        {
            // Arrange
            using var ctx = new TestContext();
            ctx.AddTestAuthorization();

            // Act
            var cut = ctx.RenderComponent<Loader>(parameters =>
                parameters.Add(p => p.Loading, true)
            );

            // Assert
            cut.Find(".loader-container").MarkupMatches(
                "<aside class=\"loader-container fade show\">" +
                    "<div class=\"loader flex-fill flex-center\">" +
                    "<i class=\"loader-icon las la-book\"></i>" +
                    "<div class=\"loader-border\"></div>" +
                    "</div>" +
                "</div>");
        }

        [Fact]
        public void LoaderShouldRender_WhenNotLoading()
        {
            // Arrange
            using var ctx = new TestContext();
            ctx.AddTestAuthorization();

            // Act
            var cut = ctx.RenderComponent<Loader>(parameters =>
                parameters.Add(p => p.Loading, false)
            );

            // Assert
            cut.Find(".loader-container").MarkupMatches(
                "<aside class=\"loader-container fade hidden\">" +
                    "<div class=\"loader flex-fill flex-center\">" +
                    "<i class=\"loader-icon las la-book\"></i>" +
                    "<div class=\"loader-border\"></div>" +
                    "</div>" +
                "</div>");
        }
    }
}