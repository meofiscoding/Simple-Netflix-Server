using System;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Services.Common
{
    public static class CommonExtensions
    {
        public static WebApplicationBuilder AddServiceDefaults(this WebApplicationBuilder builder)
        {
            builder.Services.AddDefaultAuthentication(builder.Configuration);
            return builder;
        }
        public static IServiceCollection AddDefaultAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            // {
            //   "Identity": {
            //     "Url": "http://identity",
            //     "Audience": "basket"
            //    }
            // }

            var identityUrl = configuration.GetValue<string>("Identity:Url");

            services.AddAuthentication("Bearer").AddJwtBearer("Bearer", options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.Authority = identityUrl;
                    options.RequireHttpsMetadata = false;
                    // options.Audience = "movies";
                    options.TokenValidationParameters.ValidateAudience = false;
                });

            return services;
        }
    }
}
