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
        public void LoaderShouldRender()
        {
            // Arrange
            using var ctx = new TestContext();
            ctx.AddTestAuthorization();

            // Act
            var cut = ctx.RenderComponent<Loader>();

            // Assert
            cut.Find(".loader").MarkupMatches(
                "<div class=\"loader flex-fill flex-center\">" +
                  "<i class=\"loader-icon las la-book\"></i>" +
                  "<div class=\"loader-border\"></div>" +
                "</div>");
        }
    }
}
