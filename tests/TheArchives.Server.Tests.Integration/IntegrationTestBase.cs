using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Moq;

using TheArchives.Server.Repositories;

using Xunit;

namespace TheArchives.Server.Tests.Integration
{
    public abstract class IntegrationTestBase : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly IDictionary<Type, Mock> _the;
        protected readonly JsonSerializerOptions jsonSerializerOptions;
        protected readonly HttpClient _client;

        protected IntegrationTestBase(CustomWebApplicationFactory factory)
        {

            _the = new Dictionary<Type, Mock>();
            _client = factory.WithWebHostBuilder(builder =>
            {
                // Override configuration 
                builder.ConfigureAppConfiguration((context, configBuilder) =>
                {
                    configBuilder.AddInMemoryCollection(ConfigureConfiguration());
                });

                // Allows us to override injected services
                // See https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests#inject-mock-services
                builder.ConfigureTestServices(services =>
                {
                    // Add test auth
                    services.AddAuthentication("Test")
                        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });

                    // Add all repositories here
                    services.AddSingleton(The<IContentRepository>().Object);
                    services.AddSingleton(The<ITextRepository>().Object);
                    services.AddSingleton(The<ISearchRepository>().Object);

                    // Allow inherited classes to add stuff here
                    ConfigureDependencies(services);
                });
            })
            .CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

            // Add auth header
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

            // Set default json formatter resolver for use in tests
            jsonSerializerOptions = new JsonSerializerOptions();
            jsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            jsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        }

        protected Mock<T> The<T>() where T : class
        {
            if (!_the.ContainsKey(typeof(T)))
            {
                _the[typeof(T)] = new Mock<T>();
            }

            return (Mock<T>)_the[typeof(T)];
        }

        protected void VerifyAll()
        {
            foreach (var mockedType in _the.Keys)
            {
                _the[mockedType].VerifyAll();
            }
        }

        protected async Task<T> DeserializeResponse<T>(HttpResponseMessage response)
        {
            using var stream = await response.Content.ReadAsStreamAsync();
            return (await JsonSerializer.DeserializeAsync<T>(stream))!;
        }

        protected StringContent SerializeRequest<T>(T request)
        {
            string content;
            if (request is string)
            {
                content = (request as string)!;
            }
            else
            {
                content = JsonSerializer.Serialize(request);
            }

            return new StringContent(content, System.Text.Encoding.UTF8, "application/json");
        }

        protected virtual IDictionary<string, string> ConfigureConfiguration()
        {
            // Override this to inject any necessary test-specific configuration variables
            return new Dictionary<string, string>();
        }

        protected virtual void ConfigureDependencies(IServiceCollection services)
        {
            // Override this to inject any necessary test-specific dependencies
        }
    }
}