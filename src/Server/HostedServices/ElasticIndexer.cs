using AutoMapper;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TheArchives.Server.Repositories;

namespace TheArchives.Server.HostedServices
{
    public interface IElasticIndexer
    {
        Task IndexAsnyc(CancellationToken cancellationToken = default);
    }

    public class ElasticIndexer : IElasticIndexer
    {
        protected static SemaphoreSlim Sync = new SemaphoreSlim(1, 1); // to protect long-running index task

        private readonly ISearchRepository _searchRepository;
        private readonly IContentRepository _contentRepository;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public ElasticIndexer(ISearchRepository searchRepository, IContentRepository contentRepository, ILogger<ElasticIndexer> logger, IMapper mapper)
        {
            _searchRepository = searchRepository;
            _contentRepository = contentRepository;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task IndexAsnyc(CancellationToken cancellationToken = default)
        {
            await Sync.WaitAsync(cancellationToken);

            try {
                if (!await _searchRepository.IndexExistsAsync(cancellationToken)) {
                    _logger.LogInformation("Creating Elastic index");
                    await _searchRepository.CreateIndexAsync(cancellationToken);
                }

                var expectedCount = await _contentRepository.CountAsync(cancellationToken);
                var actualCount = await _searchRepository.CountAsync(cancellationToken);
                if (expectedCount != actualCount) {
                    var count = 0;
                    var page = 1;
                    var pageSize = 100;
                    while (count < expectedCount) {
                        _logger.LogInformation("Indexing elastic documents {Count} / {Total}", count, expectedCount);
                        var docs = await _contentRepository.ListAsync(page++, pageSize, cancellationToken);
                        await _searchRepository.IndexAsync(docs.Select(_mapper.Map<Models.Elastic.Content>), cancellationToken);
                        count += docs.Count;
                    }
                }

                _logger.LogInformation("Elastic index is up to date");
            }
            finally {
                Sync.Release();
            }
        }
    }
}
