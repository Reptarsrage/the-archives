namespace TheArchives.Server.Repositories
{
    public interface ITextRepository
    {
        Task<string> ReadAsync(string path, CancellationToken cancellationToken = default);
    }

    public class TextRepository : ITextRepository
    {
        public async Task<string> ReadAsync(string path, CancellationToken cancellationToken = default)
            => await File.ReadAllTextAsync(path, cancellationToken);
    }
}