using System.Threading.Tasks;

using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.JSInterop;

using Moq;

namespace TheArchives.Client.Tests.Unit.Mocks
{
    public class MockSignOutSessionStateManager : SignOutSessionStateManager
    {
        public MockSignOutSessionStateManager() : base(new Mock<IJSRuntime>().Object)
        {
        }


        public override ValueTask SetSignOutState()
        {
            return new ValueTask(Task.CompletedTask);
        }


        public override Task<bool> ValidateSignOutState()
        {
            return Task.FromResult(true);
        }
    }
}