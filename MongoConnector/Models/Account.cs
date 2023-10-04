using AspNetCore.Identity.MongoDbCore.Models;
using MongoDbGenericRepository.Attributes;

namespace MongoConnector.Models
{
    [CollectionName(MongoCollections.Accounts)]
    public class Account : MongoIdentityUser<Guid>
    {
        // Check how user registered to the system
        public string Provider { get; set; } = null!;
        // Refresh the token without relogin
        public string? RefreshToken { get; set; }
        // How long we can use refresh token
        public DateTimeOffset RefreshTokenExpiryTime { get; set; }
    }
}
