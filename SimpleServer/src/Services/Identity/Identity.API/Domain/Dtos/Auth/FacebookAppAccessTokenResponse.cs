using System.Text.Json.Serialization;

namespace Identity.API.Domain.Dtos.Auth;

public class FacebookAppAccessTokenResponse
{
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }
    
    [JsonPropertyName("token_type")]
    public string? TokenType { get; set; }
}