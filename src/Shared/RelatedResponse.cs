namespace TheArchives.Shared
{
    public record RelatedResponse(IEnumerable<Content> Results, int Page, int PageSize, long Total, long TimeTaken)
        : SearchResponseBase(Results, Page, PageSize, Total, TimeTaken);
}
