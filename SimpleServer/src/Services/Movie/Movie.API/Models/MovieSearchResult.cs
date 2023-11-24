using System;

namespace Movie.API.Models
{
    public class MovieSearchResult
    {
        public List<DataResult> Data { get; set; } = new();
        public int TotalResults { get; set; }
    }

    public class DataResult
    {
        public string Id { get; set; } = string.Empty;
        public string Poster { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
