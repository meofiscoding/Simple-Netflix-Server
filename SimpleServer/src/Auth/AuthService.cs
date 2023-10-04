using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using MongoConnector.Models;
using SimpleServer.Configuration;

namespace SimpleServer.src.Auth
{
    public class AuthService : IAuthService
    {

        private readonly JwtConfig _jwtConfig;
        private readonly UserManager<User> _userManager;

        public AuthService(IOptions<JwtConfig> jwtConfig, UserManager<User> userManager)
        {
            _jwtConfig = jwtConfig.Value;
            _userManager = userManager;
        }

        public async Task<JwtSecurityToken> CreateToken(User user)
        {
            var authClaims = await GetClaims(user);
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfig.Secret!));

            var token = new JwtSecurityToken(
                issuer: _jwtConfig.ValidIssuer,
                audience: _jwtConfig.ValidAudience,
                expires: DateTime.Now.AddHours(1),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256));

            return token;
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private async Task<List<Claim>> GetClaims(User user)
        {
            var authClaims = new List<Claim>
        {
            new(ClaimTypes.Sid, Guid.NewGuid().ToString()),
            new(ClaimTypes.Name, user.UserName!),
            new(ClaimTypes.Email, user.Email!),
            new(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

            var userRoles = await _userManager.GetRolesAsync(user);

            if (userRoles.Any())
            {
                authClaims.AddRange(userRoles.Select(userRole => new Claim(ClaimTypes.Role, userRole)));
            }

            return authClaims;
        }
    }
}