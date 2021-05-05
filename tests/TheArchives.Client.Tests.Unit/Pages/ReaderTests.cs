using Bunit;
using Bunit.TestDoubles;
using Microsoft.AspNetCore.Components;
using RichardSzalay.MockHttp;
using System.Linq;
using TheArchives.Client.Tests.Unit.Extensions;
using TheArchives.Shared;
using Xunit;

namespace TheArchives.Client.Tests.Unit.Pages
{
    public class ReaderTests
    {
        [Fact]
        public void ShouldDisplayContentReaderWithTitleAndText()
        {
            // Arrange
            var expectedContentId = 0xbeef;
            var expectedPath = $"/reader/{expectedContentId}";
            var expectedText = string.Join(" ", Enumerable.Range(0, 100).Select(i => "Expected Text"));

            using var ctx = new TestContext();
            ctx.AddTestAuthorization();
            var mockSignOutManager = ctx.Services.AddSignOutManager();
            var mockNavigationManager = ctx.Services.AddMockNavigationManager(expectedPath);
            var mockHttpClient = ctx.Services.AddMockHttpClient();

            mockNavigationManager.SetUri(
                $"http://localhost:5000");

            mockHttpClient.When($"/api/content/text/{expectedContentId}").RespondText(expectedText);

            mockHttpClient.When($"/api/content/{expectedContentId}").RespondJson(
                new Content(0, "BRAND", "TITLE", "DESC", "AUTHOR", new Tag[0]));

            ctx.JSInterop.SetupVoid("interopFunctions.observeHiddenRefResized", IsElementReference);
            ctx.JSInterop.SetupVoid("interopFunctions.observeReaderRefResized", IsElementReference);
            ctx.JSInterop.SetupVoid("interopFunctions.unObserveHiddenRefResized");
            ctx.JSInterop.SetupVoid("interopFunctions.unObserveReaderRefResized");
            ctx.JSInterop.Setup<BoundingClientRect?>("interopFunctions.getBoundingClientRect", IsElementReference).SetResult(new BoundingClientRect { Width = 500 });

            // Act
            var cut = ctx.RenderComponent<Client.Pages.Reader>(parameters =>
                parameters.Add(p => p.ContentId, expectedContentId));
            cut.WaitForState(() => !string.IsNullOrWhiteSpace(cut.Find(".reader").TextContent));

            // Assert
            Assert.Contains("TITLE", cut.Find(".reader").TextContent);
            Assert.Contains("Expected Text", cut.Find(".reader").TextContent);
        }

        private bool IsElementReference(JSRuntimeInvocation t)
            => t.Arguments.Count == 1 && t.Arguments[0] is ElementReference;
    }
}
