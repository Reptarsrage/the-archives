using Bunit;
using Bunit.TestDoubles;

using RichardSzalay.MockHttp;

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
            var mockHttpClient = ctx.Services.AddMockHttpClient();

            mockHttpClient.When($"/api/content/count").RespondJson(0);
            mockHttpClient.When($"/api/tags/count").RespondJson(0);
            mockHttpClient.When($"/api/authors/count").RespondJson(0);
            mockHttpClient.When($"/api/brands/count").RespondJson(0);

            // Act
            var cut = ctx.RenderComponent<Client.Pages.Index>();
            var searchForm = cut.Find("form");
            var searchInput = cut.Find("input[type=\"search\"]");
            searchInput.Input(expectedSearch);
            searchForm.Submit();

            // Assert
            Assert.Equal(
                $"/search?Search={System.Uri.EscapeDataString(expectedSearch)}",
                mockNavigationManager.NavigateToLocation);
        }
    }
}