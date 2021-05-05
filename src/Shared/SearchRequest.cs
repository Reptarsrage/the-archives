namespace TheArchives.Shared
{
    public record SearchRequest(int? Seed = null, int Page = 0, int PageSize = 20, string? Search = null);
}
