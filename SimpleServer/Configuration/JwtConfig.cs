using System;

namespace SimpleServer.Configuration
{
    public class JwtConfig
    {
        public const string Position = "Jwt";

        public string? ValidAudience { get; set; }
        public string? ValidIssuer { get; set; }
        public string? Secret { get; set; }
    }
}
