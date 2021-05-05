using Bunit;
using Bunit.TestDoubles;
using TheArchives.Client.Tests.Unit.Extensions;
using Xunit;

namespace TheArchives.Client.Tests.Unit.Pages
{
    public class IndexTests
    {
        [Fact]
        public void ShouldNavigateToSearchOnSubmit()
        {
            // Arrange
            var expectedSearch = "EXPECTED SEARCH";
            using var ctx = new TestContext();
            ctx.AddTestAuthorization();
            var mockSignOutManager = ctx.Services.AddSignOutManager();
            var mockNavigationManager = ctx.Services.AddMockNavigationManager("/");

            // Act
            var cut = ctx.RenderComponent<Client.Pages.Index>();
            var searchForm = cut.Find("form");
            var searchInput = cut.Find("input[type=\"search\"]");
            searchInput.Change(expectedSearch);
            searchForm.Submit();

            // Assert
            Assert.Equal(
                $"/search?Search={System.Uri.EscapeDataString(expectedSearch)}",
                mockNavigationManager.NavigateToLocation);
        }
    }
}
