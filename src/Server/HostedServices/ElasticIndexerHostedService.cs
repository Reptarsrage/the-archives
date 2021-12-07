namespace TheArchives.Server.HostedServices
{
    /// <summary>
    /// In chare of ensuring Elastic is up-to-date on startup
    /// </summary>
    public class ElasticIndexerHostedService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private IServiceScope? serviceScope;

        public ElasticIndexerHostedService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Create a new scope to retrieve scoped services
            serviceScope = _serviceProvider.CreateScope();

            // Ignore if testing
            var environment = serviceScope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
            if (!environment.IsEnvironment("Testing"))
            {
                // Make sure all content is indexed
                var elasticIndexer = serviceScope.ServiceProvider.GetRequiredService<IElasticIndexer>();
                elasticIndexer.IndexAsnyc(cancellationToken); // Fire and forget
            }

            // Process async
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            if (serviceScope != null)
            {
                serviceScope.Dispose();
                serviceScope = null;
            }

            return Task.CompletedTask;
        }
    }
}