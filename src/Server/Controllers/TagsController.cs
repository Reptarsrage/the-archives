using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using TheArchives.Server.Repositories;

namespace TheArchives.Server.Controllers
{
    [ApiController]
    [Route("/api/[Controller]")]
    [Authorize]
    public class TagsController : ControllerBase
    {
        private readonly ILogger<TagsController> _logger;
        private readonly IContentRepository _contentRepository;
        private readonly IMemoryCache _memoryCache;

        public TagsController(ILogger<TagsController> logger,
            IContentRepository contentRepository, IMemoryCache memoryCache)
        {
            _logger = logger;
            _contentRepository = contentRepository;
            _memoryCache = memoryCache;
        }

        [HttpGet("count")]
        [ResponseCache(Duration = 86400)]
        public async Task<ActionResult<long>> GetTagsCount(CancellationToken cancellationToken = default)
        {
            return await _memoryCache.GetOrCreateAsync($"{nameof(TagsController)}_{nameof(GetTagsCount)}", async (entry) =>
            {
                entry.SlidingExpiration = System.TimeSpan.FromHours(1);
                return await _contentRepository.CountTagsAsync(cancellationToken);
            });
        }
    }
}
