using System;
using EventBus.Message.Common.Enum;

namespace EventBus.Message.Events
{
    public class MovieCrawlExportEvent : IntegrationEvent
    {
        public string MovieName { get; set; } = string.Empty;

        public string? Poster { get; set; }

        public string Status { get; set; } = "";

        public Dictionary<string, string> StreamingUrls { get; set; } = new Dictionary<string, string>();

        public string? UrlDetail { get; set; }

        public Category MovieCategory { get; set; }

        public string? Description { get; set; }

        public List<Tag> Tags { get; set; } = new List<Tag>();
    }
}
