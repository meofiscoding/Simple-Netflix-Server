using CrawlData.Enum;

namespace CrawlData.Model
{
    public class MovieItem
    {
        public string MovieName { get; set; } = string.Empty;

        public string? Poster { get; set; }

        public string Status { get; set; } = "";

        public Dictionary<int, string> StreamingUrls { get; set; } = new Dictionary<int, string>();

        public string? TrailerUrl { get; set; }

        public string? UrlDetail { get; set; }

        public Category MovieCategory { get; set; }

        public string? Description { get; set; }

        public List<Tag> Tags { get; set; } = new List<Tag>();

        // keep track all remain episode that didn't upload to GCS
        public List<int> RemainEpisodes { get; set; } = new List<int>();
    }
}
