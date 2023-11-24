using System;

namespace Movie.API.Models
{
    public class MovieSearchQueryModel
    {
        public int? Category { get; set; }
        public string? Query { get; set; } = "";

        public int Page { get; set; }
    }
}
