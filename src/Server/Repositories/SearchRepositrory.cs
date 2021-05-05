using Microsoft.Extensions.Options;
using Nest;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TheArchives.Server.Extensions;
using TheArchives.Server.Models;
using TheArchives.Server.Models.Elastic;

namespace TheArchives.Server.Repositories
{
    /// <summary>
    /// Allows for indexing content and searching
    /// </summary>
    public interface ISearchRepository
    {
        /// <summary>
        /// Creates the indes
        /// </summary>
        Task CreateIndexAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if index exists
        /// </summary>
        Task<bool> IndexExistsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets count of indexed documents
        /// </summary>
        Task<long> CountAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Indexes given content items
        /// </summary>
        /// <param name="content">The content to be indexed</param>
        Task IndexAsync(IEnumerable<Content> content, CancellationToken cancellationToken = default);

        /// <summary>
        /// Indexes a single content item
        /// </summary>
        /// <param name="content">The content to be indexed</param>
        Task IndexAsync(Content content, CancellationToken cancellationToken = default);

        /// <summary>
        /// Performs a search request
        /// </summary>
        /// <param name="searchRequest">Model containing all necessary info to form a search query</param>
        /// <returns>Content results</returns>
        Task<ISearchResponse<Content>> SearchAsync(int page, int pageSize, string searchRequest, int seed, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get related content
        /// </summary>
        Task<ISearchResponse<Content>> MoreLikeThisAsync(int documentId, int page, int pageSize, CancellationToken cancellationToken = default);
    }

    /// <inheritdoc cref="ISearchRepository" />
    public class SearchRepository : ISearchRepository
    {
        private readonly IElasticClient _elasticClient;
        private readonly ElasticOptions _elasticOptions;

        public SearchRepository(IElasticClient elasticClient, IOptions<ElasticOptions> options)
        {
            _elasticClient = elasticClient;
            _elasticOptions = options.Value;
        }

        /// <inheritdoc cref="ISearchRepository.CreateIndexAsync" />
        public async Task CreateIndexAsync(CancellationToken cancellationToken = default) => await _elasticClient.CreateCustomIndex(_elasticOptions.DefaultIndex, cancellationToken);

        /// <inheritdoc cref="ISearchRepository.IndexExistsAsync" />
        public async Task<bool> IndexExistsAsync(CancellationToken cancellationToken = default)
        {
            var result = await _elasticClient.Indices.ExistsAsync(_elasticOptions.DefaultIndex, null, cancellationToken);
            return result.Exists;
        }

        /// <inheritdoc cref="ISearchRepository.CountAsync" />
        public async Task<long> CountAsync(CancellationToken cancellationToken = default)
        {
            var result = await _elasticClient.CountAsync<Content>(null, cancellationToken);
            return result.Count;
        }

        /// <inheritdoc cref="ISearchRepository.IndexAsync(IEnumerable{Content})" />
        public async Task IndexAsync(IEnumerable<Content> content, CancellationToken cancellationToken = default)
        {
            await _elasticClient.IndexManyAsync(content, null, cancellationToken);
        }

        /// <inheritdoc cref="ISearchRepository.IndexAsync(Content)" />
        public async Task IndexAsync(Content content, CancellationToken cancellationToken = default)
        {
            await _elasticClient.IndexDocumentAsync(content, cancellationToken);
        }

        /// <inheritdoc cref="ISearchRepository.SearchAsync(SearchDomainModel)" />
        public async Task<ISearchResponse<Content>> SearchAsync(int page, int pageSize, string searchRequest, int seed, CancellationToken cancellationToken = default)
        {
            // Search
            if (!string.IsNullOrEmpty(searchRequest)) {
                return await _elasticClient.SearchAsync<Content>(s => s
                   .TrackTotalHits()
                   .Query(q => q.QueryString(d => d.Query(searchRequest)))
                   .From(page * pageSize)
                   .Size(pageSize), cancellationToken);
            }

            // Default to random order when not searching
            return await _elasticClient.SearchAsync<Content>(s => s
               .TrackTotalHits()
               .Query(q => q.FunctionScore(c => c
                                .Query(qq => qq.MatchAll())
                                .Functions(f => f.RandomScore(r => r.Seed(seed)))))
               .From(page * pageSize)
               .Size(pageSize), cancellationToken);
        }

        /// <inheritdoc cref="ISearchRepository.SearchAsync(SearchDomainModel)" />
        public async Task<ISearchResponse<Content>> MoreLikeThisAsync(int documentId, int page, int pageSize, CancellationToken cancellationToken = default)
        {
            return await _elasticClient.SearchAsync<Content>(s => s
               .TrackTotalHits()
               .Query(q => q.MoreLikeThis(d => d
                   .MinTermFrequency(1)
                   .MaxQueryTerms(12)
                   .Like(e => e.Document(f => f.Id(documentId)))))
               .From(page * pageSize)
               .Size(pageSize), cancellationToken);
        }
    }
}
