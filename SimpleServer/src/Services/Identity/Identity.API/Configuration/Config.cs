using IdentityServer4;
using IdentityServer4.Models;

namespace Identity.API.Configuration
{
    public static class Config
    {

        // client want to access resources (aka scopes)
        public static IEnumerable<Client> GetClients(IConfiguration configuration)
        {
            return new List<Client>
            {
                new Client()
                {
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    ClientId = "client",
                    ClientSecrets = { new Secret("secret".Sha256()) },
                    AllowedScopes = { "movies" },
                },
                // JavaScript Client
                new Client
                {
                    ClientId = "js",
                    ClientName = "Simple Netflix SPA OpenId Client",
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    AllowAccessTokensViaBrowser = true,
                    ClientSecrets = { new Secret("secret".Sha256()) },
                    RedirectUris =           { $"{configuration["SpaClient"]}/" },
                    RequireConsent = false,
                    PostLogoutRedirectUris = { $"{configuration["SpaClient"]}/" },
                    AllowedCorsOrigins =     { $"{configuration["SpaClient"]}" },
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "movies"
                    },
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
                //new Client
                //{
                //    ClientId = "movieswaggerui",
                //    ClientName = "Movie Swagger UI",
                //    AllowedGrantTypes = GrantTypes.Implicit,
                //    AllowAccessTokensViaBrowser = true,

                //    RedirectUris = { $"{configuration["BasketApiClient"]}/swagger/oauth2-redirect.html" },
                //    PostLogoutRedirectUris = { $"{configuration["BasketApiClient"]}/swagger/" },

                //    AllowedScopes =
                //    {
                //        "basket"
                //    }
                //},
            };
        }

        // Identity resources are data like user ID, name, or email address of a user
        // see: http://docs.identityserver.io/en/release/configuration/resources.html
        public static IEnumerable<IdentityResource> GetResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile()
            };
        }

        // ApiResources define the apis in your system
        public static IEnumerable<ApiResource> GetApis()
        {
            return new List<ApiResource>
            {
                new ApiResource("movies", "Movie Service"),
            };
        }

        // ApiScope is used to protect the API 
        //The effect is the same as that of API resources in IdentityServer 3.x
        public static IEnumerable<ApiScope> GetApiScopes()
        {
            return new List<ApiScope>
            {
                new ApiScope("movies", "Movie Service"),
            };
        }
    }
}

