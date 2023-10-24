using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Movie.API.Models
{
    public class MovieInformation
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string BackdropPath { get; set; } = string.Empty;

        public List<string> Genres { get; set; } = new();

        public string OriginalTitle { get; set; } = string.Empty;

        public string Overview { get; set; } = string.Empty;

        public string PosterPath { get; set; } = string.Empty;

        public string ReleaseDate { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

    }
}

