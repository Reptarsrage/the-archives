using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TheArchives.Server.Data;
using TheArchives.Server.Models.Dto;

namespace TheArchives.Server.Repositories
{
    public interface IContentRepository
    {
        Task<List<Content>> ListAsync(int page, int pageSize, CancellationToken cancellationToken = default);

        Task<Content?> GetAsync(int contentId, CancellationToken cancellationToken = default);

        Task<long> CountAsync(CancellationToken cancellationToken = default);
    }

    public class ContentRepository : IContentRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public ContentRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Content?> GetAsync(int contentId, CancellationToken cancellationToken = default)
            => await _dbContext.Content!
                .Include(c => c.Tags)
                .SingleOrDefaultAsync(b => b.ContentId == contentId, cancellationToken);

        public async Task<long> CountAsync(CancellationToken cancellationToken = default)
            => await _dbContext.Content!.CountAsync(cancellationToken);

        public async Task<List<Content>> ListAsync(int page, int pageSize, CancellationToken cancellationToken = default)
            => await _dbContext.Content!
                .OrderBy(b => b.ContentId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(c => c.Tags)
                .ToListAsync(cancellationToken);

    }
}