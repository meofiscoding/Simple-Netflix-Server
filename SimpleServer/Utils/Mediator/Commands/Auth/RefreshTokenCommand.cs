using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoConnector.Models;
using SimpleServer.Configuration;
using SimpleServer.src.Auth;
using SimpleServer.src.Auth.DTOs;

namespace SimpleServer.Utils.Mediator.Commands.Auth
{
    public class RefreshTokenCommand : IRequest<RefreshTokenDto>
    {
        public RefreshTokenCommand(RefreshTokenDto command)
        {
            Command = command;
        }

        public RefreshTokenDto Command { get; set; }

        public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, RefreshTokenDto>
        {
            private readonly UserManager<Account> _userManager;
            private readonly JwtConfig _jwtConfiguration;
            private readonly IAuthService _authService;

            public RefreshTokenCommandHandler(
                UserManager<Account> userManager,
                IOptions<JwtConfig> jwtConfig,
                IAuthService authService)
            {
                _userManager = userManager;
                _jwtConfiguration = jwtConfig.Value;
                _authService = authService;
            }

            public async Task<RefreshTokenDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
            {
                var accessToken = request.Command.AccessToken;
                var refreshToken = request.Command.RefreshToken;

                var principal = GetPrincipalFromExpiredToken(accessToken) ?? throw new Exception("Invalid access token or refresh token");
                var username = principal.Identity!.Name!;

                var user = await _userManager.FindByNameAsync(username);

                if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.Now)
                {
                    throw new Exception("Invalid access token or refresh token");
                }

                var newAccessToken = await _authService.CreateToken(user);
                var newRefreshToken = GenerateRefreshToken();

                user.RefreshToken = newRefreshToken;
                await _userManager.UpdateAsync(user);

                return new RefreshTokenDto
                {
                    AccessToken = new JwtSecurityTokenHandler().WriteToken(newAccessToken),
                    RefreshToken = newRefreshToken
                };
            }

            private static string GenerateRefreshToken()
            {
                var randomNumber = new byte[64];
                using var rng = RandomNumberGenerator.Create();
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }

            private ClaimsPrincipal? GetPrincipalFromExpiredToken(string? token)
            {
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfiguration.Secret!)),
                    ValidateLifetime = false
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

                if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                    throw new SecurityTokenException("Invalid token");

                return principal;
            }
        }
    }
}
