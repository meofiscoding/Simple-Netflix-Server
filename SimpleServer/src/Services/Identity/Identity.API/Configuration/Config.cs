using System;
using Duende.IdentityServer;
using Duende.IdentityServer.Models;

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
            // new Client()
            // {
            //     ClientId = "angular",

            //     AllowedGrantTypes = GrantTypes.Code,
            //     RequirePkce = true,
            //     RequireClientSecret = false,

            //     RedirectUris = { "http://localhost:4200" },
            //     PostLogoutRedirectUris = { "http://localhost:4200" },
            //     AllowedCorsOrigins = { "http://localhost:4200" },

            //     AllowedScopes = {
            //         IdentityServerConstants.StandardScopes.OpenId,
            //         "movies",
            //     },

            //     AllowAccessTokensViaBrowser = true,
            //     RequireConsent = false,
            // },
            new Client()
            {
                ClientName = "Angular-Client",
                ClientId = "angular-client",
                AllowedGrantTypes = GrantTypes.Code,
                RedirectUris = new List<string>{ "http://localhost:4200/signin-callback", "http://localhost:4200/assets/silent-callback.html" },
                RequirePkce = true,
                AllowAccessTokensViaBrowser = true,
                AllowedScopes =
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    "movies"
                },
                AllowedCorsOrigins = { "http://localhost:4200" },
                RequireClientSecret = false,
                PostLogoutRedirectUris = new List<string> { "http://localhost:4200/signout-callback" },
                RequireConsent = false,
                AccessTokenLifetime = 600
            }
        };

        public static IEnumerable<ApiScope> ApiScopes =>
            new List<ApiScope>
            {
                new ApiScope("movies", "Movie Service"),
            };

        public static IEnumerable<ApiResource> ApiResources =>
            new List<ApiResource>
            {
                new ApiResource("movies", "Movie Service"){
                    Scopes = { "movies" }
                }
            };
        public static IEnumerable<IdentityResource> IdentityResources =>
            new List<IdentityResource>
            {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile()
            };
    }
}
