using System;
using AspNetCore.Identity.MongoDbCore.Models;
using MongoDbGenericRepository.Attributes;

namespace MongoConnector.Models
{
    [CollectionName(MongoCollections.Roles)]
    public class Role : MongoIdentityRole<Guid>
    {
        public const string Admin = "Admin";
        public const string User = "User";
    }
}

