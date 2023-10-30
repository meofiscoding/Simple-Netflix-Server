using System;
using IdentityServer4;
using IdentityServer4.Models;

namespace Identity.API.Configuration
{
    public static class Config
    {
        public static IEnumerable<Client> Clients =>
        new List<Client>
        {
            new Client()
            {
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                ClientId = "client",
                ClientSecrets = { new Secret("secret".Sha256()) },
                AllowedScopes = { "movies" },
            },
            new Client()
            {
                ClientId = "postman",
                AllowedGrantTypes = GrantTypes.Code,
                RequirePkce = true,
                RequireClientSecret = false,
                RedirectUris = { "https://www.thunderclient.com/oauth/callback" },
                AllowedScopes =
                {
                    "movies",
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile
                },
                AllowAccessTokensViaBrowser = true,
                AllowOfflineAccess = true,
                RequireConsent = false
            },
            new Client()
            {
                ClientId = "angular",

                AllowedGrantTypes = GrantTypes.Code,
                RequirePkce = true,
                RequireClientSecret = false,

                RedirectUris = { "http://localhost:4200" },
                PostLogoutRedirectUris = { "http://localhost:4200" },
                AllowedCorsOrigins = { "http://localhost:4200" },

                AllowedScopes = {
                    IdentityServerConstants.StandardScopes.OpenId,
                    "movies",
                },

                AllowAccessTokensViaBrowser = true,
                RequireConsent = false,
            },
            new Client()
            {
                ClientId = "angular",

                AllowedGrantTypes = GrantTypes.Code,
                RequirePkce = true,
                RequireClientSecret = false,

                RedirectUris = { "https://simplenetflix.vercel.app" },
                PostLogoutRedirectUris = { "https://simplenetflix.vercel.app" },
                AllowedCorsOrigins = { "https://simplenetflix.vercel.app" },

                AllowedScopes = {
                    IdentityServerConstants.StandardScopes.OpenId,
                    "movies",
                },

                AllowAccessTokensViaBrowser = true,
                RequireConsent = false,
            },
        };

        public static IEnumerable<ApiScope> ApiScopes =>
            new List<ApiScope>
            {
                new ApiScope("movies", "Movie Service"),
            };

        public static IEnumerable<ApiResource> ApiResources =>
            new List<ApiResource>
            {
                new ApiResource("movies", "Movie Service")
            };
        public static IEnumerable<IdentityResource> IdentityResources =>
            new List<IdentityResource>
            {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile()
            };
    }
}
