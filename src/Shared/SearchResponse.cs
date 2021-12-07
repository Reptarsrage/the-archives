namespace TheArchives.Shared
{
    public record SearchResponse(IEnumerable<Content> Results, int Page, int PageSize, long Total, long TimeTaken, int Seed, string Search)
        : SearchResponseBase(Results, Page, PageSize, Total, TimeTaken);
}
