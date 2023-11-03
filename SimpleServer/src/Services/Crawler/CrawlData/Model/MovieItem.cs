using CrawlData.Enum;

namespace CrawlData.Model
{
    public class MovieItem
    {
        public string? Poster { get; set; }

        public string Status { get; set; } = "";

        public List<string> StreamingUrls { get; set; } = new List<string>();

        public string? TrailerUrl { get; set; }

        public string? UrlDetail { get; set; }

        public Category MovieCategory { get; set; }

        public string? Description { get; set; }

        public List<Tag>? Tags { get; set; }

        public string? MovieName { get; set; }
    }
}
