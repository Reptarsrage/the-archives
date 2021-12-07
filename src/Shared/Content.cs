namespace TheArchives.Shared
{
    public record Content(int ContentId, string Brand, string Title, string Description, string Author, IEnumerable<string> Tags);
}
