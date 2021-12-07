using System.ComponentModel.DataAnnotations;

using AutoMapper;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

using TheArchives.Server.Repositories;
using TheArchives.Shared;

namespace TheArchives.Server.Controllers
{
    [ApiController]
    [Route("/api/[Controller]")]
    [Authorize]
    public class SearchController : ControllerBase
    {
        private readonly ILogger<SearchController> _logger;
        private readonly ISearchRepository _searchRepository;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _memoryCache;

        public SearchController(ISearchRepository searchRepository, ILogger<SearchController> logger, IMemoryCache memoryCache, IMapper mapper)
        {
            _searchRepository = searchRepository;
            _logger = logger;
            _memoryCache = memoryCache;
            _mapper = mapper;
        }

        [HttpGet]
        [ResponseCache(Duration = 3600)]
        public async Task<ActionResult<SearchResponse>> GetSearch([FromQuery] SearchRequest request, CancellationToken cancellationToken = default)
        {
            var page = Math.Min(1000, Math.Max(0, request.Page));
            var pageSize = Math.Min(100, Math.Max(5, request.PageSize));
            var seed = request.Seed ?? GenerateSeed();
            var search = request.Search ?? string.Empty;

            var response = await _searchRepository.SearchAsync(page, pageSize, search, seed, cancellationToken);
            if (!response.IsValid)
            {
                _logger.LogWarning("Failed to get search results for {Seed}", seed);
                return StatusCode(500, "Failed to get search results");
            }

            var results = response.Documents.Select(_mapper.Map<Content>);
            return new SearchResponse(results, page, pageSize, response.Total, response.Took, seed, search);
        }

        [HttpGet("{contentId}")]
        [ResponseCache(Duration = 86400)]
        public async Task<ActionResult<RelatedResponse>> GetRelated([Required][FromRoute] int contentId, [FromQuery] RelatedRequest request, CancellationToken cancellationToken = default)
        {
            var page = Math.Min(1000, Math.Max(0, request.Page));
            var pageSize = Math.Min(100, Math.Max(5, request.PageSize));

            // Get related (cached)
            var response = await _memoryCache.GetOrCreateAsync($"{nameof(GetRelated)}_{contentId}_{page}_{pageSize}", async (entry) =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(10);
                return await _searchRepository.MoreLikeThisAsync(contentId, page, pageSize, cancellationToken);
            });

            if (!response.IsValid)
            {
                _logger.LogWarning("Failed to get related for {ContentId}", contentId);
                return StatusCode(500, "Failed to get related content");
            }

            var results = response.Documents.Select(_mapper.Map<Content>);
            return new RelatedResponse(results, page, pageSize, response.Total, response.Took);
        }

        protected virtual int GenerateSeed()
        {
            return int.Parse(DateTime.Now.ToString("yyyyMMddHH"));
        }
    }
}