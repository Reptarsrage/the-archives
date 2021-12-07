using System;
using System.Collections.Generic;

using Bunit;
using Bunit.TestDoubles;

using RichardSzalay.MockHttp;

using TheArchives.Client.Tests.Unit.Extensions;
using TheArchives.Shared;

using Xunit;

namespace TheArchives.Client.Tests.Unit.Pages
{
    public class SearchTests
    {
        [Fact]
        public void ShouldDisplaySearchResults()
        {
            // Arrange
            var expectedSearch = "EXPECTED SEARCH";
            var expectedPath = $"/search?search={Uri.EscapeDataString(expectedSearch)}";
            var expectedResults = new List<Content> {
                new Content(0, "BRAND", "TITLE", "DESC", "AUTHOR", new string[0]),
                new Content(1, "BRAND", "TITLE", "DESC", "AUTHOR", new string[0]),
                new Content(2, "BRAND", "TITLE", "DESC", "AUTHOR", new string[0]),
            };

            using var ctx = new TestContext();
            ctx.AddTestAuthorization();
            var mockSignOutManager = ctx.Services.AddSignOutManager();
            var mockNavigationManager = ctx.Services.AddMockNavigationManager(expectedPath);
            var mockHttpClient = ctx.Services.AddMockHttpClient();

            mockNavigationManager.SetUri($"http://localhost:5000{expectedPath}");
            mockHttpClient.When("/api/search*").RespondJson(
                new SearchResponse(expectedResults, 0, 20, 3, 10, 0xbeef, expectedSearch));

            // Act
            var cut = ctx.RenderComponent<Client.Pages.Search>();
            cut.WaitForState(() => cut.FindAll("li").Count == expectedResults.Count, TimeSpan.FromMilliseconds(500));
        }
    }
}