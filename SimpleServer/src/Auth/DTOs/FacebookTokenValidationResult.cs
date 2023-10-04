using System;
using System.Text.Json.Serialization;

namespace SimpleServer.src.Auth.DTOs
{
    public class FacebookTokenValidationResult
    {
        [JsonPropertyName("data")]
        public FacebookTokenValidationData Data { get; set; } = null!;
    }
}
