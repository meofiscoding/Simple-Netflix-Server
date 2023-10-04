using System;

namespace SimpleServer.src.Auth.DTOs
{
    public class LoginResponseDto
    {
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
    }
}
