using CrawlData.Enum;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CrawlData.Model
{
    public class MovieItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string MovieName { get; set; } = string.Empty;

        public string? Poster { get; set; }

        public string Status { get; set; } = "";

        public Dictionary<string, string> StreamingUrls { get; set; } = new Dictionary<string, string>();

        public string? UrlDetail { get; set; }

        public Category MovieCategory { get; set; }

        public string? Description { get; set; }

        public List<Tag> Tags { get; set; } = new List<Tag>();

        public bool IsAvailable { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
