using System;

namespace SimpleServer.src.Auth.DTOs
{
    public class RefreshTokenDto
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
    }
}
