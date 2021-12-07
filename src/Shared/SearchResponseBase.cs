namespace TheArchives.Shared
{
    public abstract record SearchResponseBase(IEnumerable<Content> Results, int Page, int PageSize, long Total, long TimeTaken);
}
