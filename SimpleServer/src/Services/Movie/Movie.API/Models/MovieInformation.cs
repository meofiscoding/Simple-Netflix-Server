using EventBus.Message.Common.Enum;
using EventBus.Message.Events;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Movie.API.Models
{
    public class MovieInformation : IntegrationEvent
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public int WatchingCount { get; set; }

        public string MovieName { get; set; } = string.Empty;

        public string? Poster { get; set; }

        public string Status { get; set; } = "";

        public Dictionary<string, string> StreamingUrls { get; set; } = new Dictionary<string, string>();

        public Category MovieCategory { get; set; }

        public string? Description { get; set; }

        public List<Tag> Tags { get; set; } = new List<Tag>();

        // TODO: delete all the fields below
        public string UrlDetail { get; set; }
        public bool IsAvailable { get; set; }

        // Keep track how many episode pushed to GCS
        public int AvailableEpisode { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

    }
}

