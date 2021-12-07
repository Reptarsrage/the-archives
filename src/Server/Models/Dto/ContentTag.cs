namespace TheArchives.Server.Models.Dto
{
    public class ContentTag
    {
        public int ContentId { get; set; }
        public Content? Content { get; set; }

        public int TagId { get; set; }
        public Tag? Tag { get; set; }
    }
}