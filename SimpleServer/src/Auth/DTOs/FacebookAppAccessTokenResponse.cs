using System;
using System.Text.Json.Serialization;

namespace SimpleServer.src.Auth.DTOs
{
    public class FacebookAppAccessTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string access_token { get; set; } = string.Empty;

        [JsonPropertyName("token_type")]
        public string token_type { get; set; } = string.Empty;
    }
}
