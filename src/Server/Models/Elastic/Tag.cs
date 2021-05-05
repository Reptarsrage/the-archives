using System.Collections.Generic;

namespace TheArchives.Server.Models.Elastic
{
    public class Tag
    {
        public int TagId { get; set; }

        public string? Label { get; set; }

        public int Count { get; set; }

        public IEnumerable<Content>? Content { get; set; }
    }
}
