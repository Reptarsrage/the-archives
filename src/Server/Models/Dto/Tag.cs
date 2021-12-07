namespace TheArchives.Server.Models.Dto
{
    public class Tag
    {
        public int TagId { get; set; }

        public string Label { get; set; } = null!;

        public int Count { get; set; }

        public ICollection<Content>? Content { get; set; }

        public List<ContentTag>? ContentTags { get; set; }
    }
}
