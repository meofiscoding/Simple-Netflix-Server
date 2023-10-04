using AspNetCore.Identity.MongoDbCore.Models;
using MongoDbGenericRepository.Attributes;

namespace MongoConnector.Models
{
    [CollectionName(MongoCollections.Accounts)]
    public class Account : MongoIdentityUser<Guid>
    {
        public string Provider { get; set; } = null!;
        public string? RefreshToken { get; set; }
        public DateTimeOffset RefreshTokenExpiryTime { get; set; }
    }
}
