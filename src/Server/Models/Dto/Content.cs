using System.Collections.Generic;

namespace TheArchives.Server.Models.Dto
{
    public class Content
    {
        public int ContentId { get; set; }

        public string Brand { get; set; } = null!;

        public string Title { get; set; } = null!;

        public string Description { get; set; } = null!;

        public string Path { get; set; } = null!;

        public string Url { get; set; } = null!;

        public string Author { get; set; } = null!;

        public ICollection<Tag>? Tags { get; set; }

        public List<ContentTag>? ContentTags { get; set; }
    }
}
