using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TheArchives.Server.Models;
using TheArchives.Server.Repositories;
using TheArchives.Shared;

namespace TheArchives.Server.Controllers
{
    [ApiController]
    [Route("/api/[Controller]")]
    [Authorize]
    public class ContentController : ControllerBase
    {
        private readonly ILogger<ContentController> _logger;
        private readonly IContentRepository _contentRepository;
        private readonly ITextRepository _textRepository;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _memoryCache;
        private readonly IOptions<ContentOptions> _options;

        public ContentController(ITextRepository textRepository,
            ILogger<ContentController> logger, IMemoryCache memoryCache, IMapper mapper,
            IContentRepository contentRepository, IOptions<ContentOptions> options)
        {
            _textRepository = textRepository;
            _logger = logger;
            _memoryCache = memoryCache;
            _mapper = mapper;
            _contentRepository = contentRepository;
            _options = options;
        }

        [HttpGet("text/{contentId}")]
        [ResponseCache(Duration = 86400)]
        public async Task<ActionResult<string>> GetContentTextById([FromRoute][Required] int contentId, CancellationToken cancellationToken = default)
        {
            // Get document from database (cached)
            var contentDto = await _memoryCache.GetOrCreateAsync($"{nameof(ContentController)}_{contentId}", async (entry) =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(10);
                return await _contentRepository.GetAsync(contentId, cancellationToken);
            });

            if (contentDto == null) {
                return NotFound();
            }

            // Read document text (cached)
            var documentPath = Path.Join(_options.Value.BaseDir!, contentDto.Path!);
            return await _memoryCache.GetOrCreateAsync($"{nameof(ContentController)}_{documentPath}", async (entry) =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(10);
                return await _textRepository.ReadAsync(documentPath, cancellationToken);
            });
        }

        [HttpGet("{contentId}")]
        [ResponseCache(Duration = 86400)]
        public async Task<ActionResult<Content>> GetContentById([FromRoute][Required] int contentId, CancellationToken cancellationToken = default)
        {
            // Get document from database (cached)
            var contentDto = await _memoryCache.GetOrCreateAsync($"{nameof(ContentController)}_{contentId}", async (entry) =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(10);
                return await _contentRepository.GetAsync(contentId, cancellationToken);
            });

            if (contentDto == null) {
                return NotFound();
            }

            return _mapper.Map<Content>(contentDto);
        }
    }
}
