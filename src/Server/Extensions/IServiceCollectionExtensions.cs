using Elasticsearch.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nest;
using System;
using TheArchives.Server.Models.Elastic;

namespace TheArchives.Server.Extensions
{
    public static class IServiceCollectionExtensions
    {
        /// <summary>
        /// Bootstraps Elastic search client
        /// </summary>
        public static IServiceCollection AddElasticsearch(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            if (webHostEnvironment.IsEnvironment("Testing")) {
                return services;
            }

            var url = configuration["Elastic:Uri"];
            var defaultIndex = configuration["Elastic:DefaultIndex"];

            var pool = new SingleNodeConnectionPool(new Uri(url));
            var settings = new ConnectionSettings(pool)
                .RequestTimeout(TimeSpan.FromSeconds(5))
                .DefaultIndex(defaultIndex)
                .DefaultMappingFor<Content>(clrTypeMapping => clrTypeMapping
                    .IdProperty(content => content.ContentId)
                );

            var client = new ElasticClient(settings);
            services.AddSingleton<IElasticClient>(client);

            return services;
        }
    }
}
