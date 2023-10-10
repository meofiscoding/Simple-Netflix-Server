using System.Text;
using Identity.API.Db;
using Identity.API.Domain.Entities;
using Identity.API.Domain.Mediator.Commands.Auth;
using Identity.API.Domain.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace Identity.API.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection RegisterAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddIdentity<User, IdentityRole>()
            .AddRoles<IdentityRole>()
            .AddRoleManager<RoleManager<IdentityRole>>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        services.Configure<JwtConfiguration>(
            configuration.GetSection(JwtConfiguration.Position));

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidAudience = configuration["JWT:ValidAudience"],
                    ValidIssuer = configuration["JWT:ValidIssuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret"])),
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddTransient<IAuthService, AuthService>();

        services.AddAuthorization(options =>
        {
            options.AddPolicy("ElevatedRights", policy =>
                policy.RequireRole(Role.Admin));
            options.AddPolicy("StandardRights", policy =>
                policy.RequireRole(Role.Admin, Role.User));
        });

        return services;
    }
}