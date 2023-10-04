using System;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using MongoConnector.Models;

namespace SimpleServer.src.Auth
{
    public interface IAuthService
    {
        Task<JwtSecurityToken> CreateToken(Account user);
        string GenerateRefreshToken();
    }
}
