using Bunit;
using Bunit.TestDoubles;
using Microsoft.AspNetCore.Components;
using RichardSzalay.MockHttp;
using System;
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
            var expectedText = "Expected Text";

            using var ctx = new TestContext();
            ctx.AddTestAuthorization();
            var mockSignOutManager = ctx.Services.AddSignOutManager();
            var mockNavigationManager = ctx.Services.AddMockNavigationManager(expectedPath);
            var mockHttpClient = ctx.Services.AddMockHttpClient();

            mockNavigationManager.SetUri($"http://localhost:5000{expectedPath}");
            mockHttpClient.When($"/api/content/text/{expectedContentId}").RespondText(expectedText);
            mockHttpClient.When($"/api/content/{expectedContentId}").RespondJson(
                new Content(0, "BRAND", "TITLE", "DESC", "AUTHOR", new string[0]));

            ctx.JSInterop.SetupVoid("interopFunctions.observeHiddenRefResized", IsElementReference).SetVoidResult();
            ctx.JSInterop.SetupVoid("interopFunctions.observeReaderRefResized", IsElementReference).SetVoidResult();
            ctx.JSInterop.SetupVoid("interopFunctions.unObserveHiddenRefResized", IsElementReference).SetVoidResult();
            ctx.JSInterop.SetupVoid("interopFunctions.unObserveReaderRefResized", IsElementReference).SetVoidResult();
            ctx.JSInterop.SetupVoid("interopFunctions.initializeBody").SetVoidResult();
            ctx.JSInterop.SetupVoid("interopFunctions.disposeBody").SetVoidResult();
            ctx.JSInterop.SetupVoid("interopFunctions.listenToSwipes").SetVoidResult();
            ctx.JSInterop.SetupVoid("interopFunctions.unlistenToSwipes").SetVoidResult();

            // Act
            using var cut = ctx.RenderComponent<Client.Pages.Reader>(parameters => parameters.Add(p => p.ContentId, expectedContentId));
            cut.WaitForState(() => !string.IsNullOrWhiteSpace(cut.Find(".reader").TextContent), timeout: TimeSpan.FromMilliseconds(500));

            // Assert
            Assert.Contains("TITLE", cut.Find(".reader").TextContent);
            Assert.Contains("Expected Text", cut.Find(".reader").TextContent);
        }

        private bool IsElementReference(JSRuntimeInvocation t)
            => t.Arguments.Count == 1 && t.Arguments[0] is ElementReference;
    }
}
