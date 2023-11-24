using System;

namespace Movie.API.Models
{
    public class MoviePlayerModel
    {
        public List<string> StreamingUrls { get; set; } = new();
        public bool IsSeries { get; set; }
        public string Poster { get; set; } = string.Empty;
    }
}
