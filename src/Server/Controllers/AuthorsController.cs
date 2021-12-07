using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

using TheArchives.Server.Repositories;

namespace TheArchives.Server.Controllers
{
    [ApiController]
    [Route("/api/[Controller]")]
    [Authorize]
    public class AuthorsController : ControllerBase
    {
        private readonly ILogger<AuthorsController> _logger;
        private readonly IContentRepository _contentRepository;
        private readonly IMemoryCache _memoryCache;

        public AuthorsController(ILogger<AuthorsController> logger,
            IContentRepository contentRepository, IMemoryCache memoryCache)
        {
            _logger = logger;
            _contentRepository = contentRepository;
            _memoryCache = memoryCache;
        }

        [HttpGet("count")]
        [ResponseCache(Duration = 86400)]
        public async Task<ActionResult<long>> GetAuthorsCount(CancellationToken cancellationToken = default)
        {
            return await _memoryCache.GetOrCreateAsync($"{nameof(AuthorsController)}_{nameof(GetAuthorsCount)}", async (entry) =>
            {
                entry.SlidingExpiration = System.TimeSpan.FromHours(1);
                return await _contentRepository.CountAuthorsAsync(cancellationToken);
            });
        }
    }
}