using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using TheArchives.Server.Repositories;

namespace TheArchives.Server.Controllers
{
    [ApiController]
    [Route("/api/[Controller]")]
    [Authorize]
    public class BrandsController : ControllerBase
    {
        private readonly ILogger<BrandsController> _logger;
        private readonly IContentRepository _contentRepository;
        private readonly IMemoryCache _memoryCache;

        public BrandsController(ILogger<BrandsController> logger,
            IContentRepository contentRepository, IMemoryCache memoryCache)
        {
            _logger = logger;
            _contentRepository = contentRepository;
            _memoryCache = memoryCache;
        }

        [HttpGet("count")]
        [ResponseCache(Duration = 86400)]
        public async Task<ActionResult<long>> GetBrandsCount(CancellationToken cancellationToken = default)
        {
            return await _memoryCache.GetOrCreateAsync($"{nameof(BrandsController)}_{nameof(GetBrandsCount)}", async (entry) =>
            {
                entry.SlidingExpiration = System.TimeSpan.FromHours(1);
                return await _contentRepository.CountBrandsAsync(cancellationToken);
            });
        }
    }
}
