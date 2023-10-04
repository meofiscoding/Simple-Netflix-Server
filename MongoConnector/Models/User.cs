using System;
using AspNetCore.Identity.MongoDbCore.Models;

namespace MongoConnector.Models
{
    public class User : MongoIdentityUser<Guid>
    {
        public string Provider { get; set; } = null!;
        public string? RefreshToken { get; set; }
        public DateTimeOffset RefreshTokenExpiryTime { get; set; }
    }
}