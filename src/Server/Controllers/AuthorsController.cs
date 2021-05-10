using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
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

        public AuthorsController(ILogger<AuthorsController> logger,
            IContentRepository contentRepository)
        {
            _logger = logger;
            _contentRepository = contentRepository;
        }

        [HttpGet("count")]
        [ResponseCache(Duration = 86400)]
        public async Task<ActionResult<long>> GetAuthorsCount(CancellationToken cancellationToken = default)
        {
            return await _contentRepository.CountAuthorsAsync(cancellationToken);
        }
    }
}
