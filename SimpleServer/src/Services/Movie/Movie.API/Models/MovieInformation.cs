using EventBus.Message.Events;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Movie.API.Models
{
    public class MovieInformation : TransferMovieListEvent
    {
        public int WatchingCount { get; set; }
    }
}

