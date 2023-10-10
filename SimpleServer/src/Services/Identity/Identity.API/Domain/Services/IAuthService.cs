using System.IdentityModel.Tokens.Jwt;
using Identity.API.Domain.Entities;

namespace Identity.API.Domain.Services;

public interface IAuthService
{
    Task<JwtSecurityToken> CreateToken(User user);
    string GenerateRefreshToken();
}