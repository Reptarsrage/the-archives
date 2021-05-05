using System.Collections.Generic;

namespace TheArchives.Server.Models.Elastic
{
    public class Content
    {
        public int ContentId { get; set; }

        public string? Brand { get; set; }

        public string? Title { get; set; }

        public string? Description { get; set; }

        public string? Path { get; set; }

        public string? Url { get; set; }

        public string? Author { get; set; }

        public IEnumerable<Tag>? Tags { get; set; }
    }
}
