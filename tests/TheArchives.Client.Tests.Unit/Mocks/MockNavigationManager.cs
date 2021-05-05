using Microsoft.AspNetCore.Components;

namespace TheArchives.Client.Tests.Unit.Mocks
{
    public class MockNavigationManager : NavigationManager
    {
        public string? NavigateToLocation { get; private set; }

        public void SetUri(string uri) => Uri = uri;

        public new virtual void NavigateTo(string uri, bool forceLoad = false)
        {
            NavigateToCore(uri, forceLoad);
        }

        protected override void NavigateToCore(string uri, bool forceLoad)
        {
            NavigateToLocation = uri;
            Uri = $"{this.BaseUri}{uri}";
        }

        protected void EnsureInitialized(string path)
        {
            Initialize("http://localhost:5000/", $"http://localhost:5000{path}");
        }

        public MockNavigationManager(string path)
        {
            EnsureInitialized(path);
        }
    }
}
