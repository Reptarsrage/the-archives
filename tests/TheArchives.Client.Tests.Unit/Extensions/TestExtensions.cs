using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Extensions.DependencyInjection;
using RichardSzalay.MockHttp;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using TheArchives.Client.Tests.Unit.Mocks;

namespace TheArchives.Client.Tests.Unit.Extensions
{
    public static class TestExtensions
    {
        public static MockHttpMessageHandler AddMockHttpClient(this TestServiceProvider services)
        {
            var mockHttpHandler = new MockHttpMessageHandler();
            var httpClient = mockHttpHandler.ToHttpClient();
            httpClient.BaseAddress = new Uri("http://localhost");
            services.AddSingleton<HttpClient>(httpClient);
            return mockHttpHandler;
        }

        public static MockNavigationManager AddMockNavigationManager(this TestServiceProvider services, string path)
        {
            var mockNavigationManager = new MockNavigationManager(path);
            services.AddSingleton<NavigationManager>(mockNavigationManager);
            return mockNavigationManager;
        }

        public static MockSignOutSessionStateManager AddSignOutManager(this TestServiceProvider services)
        {
            var mockSignOutManager = new MockSignOutSessionStateManager();
            services.AddSingleton<SignOutSessionStateManager>(mockSignOutManager);
            return mockSignOutManager;
        }

        public static MockedRequest RespondText(this MockedRequest request, string content)
        {
            request.Respond(req =>
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(content);
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
                return response;
            });

            return request;
        }

        public static MockedRequest RespondJson<T>(this MockedRequest request, T content)
        {
            request.Respond(req =>
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(JsonSerializer.Serialize(content));
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return response;
            });
            return request;
        }

        public static MockedRequest RespondJson<T>(this MockedRequest request, Func<T> contentProvider)
        {
            request.Respond(req =>
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(JsonSerializer.Serialize(contentProvider()));
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return response;
            });
            return request;
        }
    }
}
