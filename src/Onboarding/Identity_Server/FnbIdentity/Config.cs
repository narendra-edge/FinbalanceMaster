using Duende.IdentityServer.Models;
using static Duende.IdentityServer.IdentityServerConstants;

namespace FnbIdentity
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> GetIdentityResources()

        {
            var profile = new IdentityResources.Profile();
            profile.UserClaims.Add("role");
            return new IdentityResource[]
            {
                profile,
                new IdentityResources.OpenId(),
                new IdentityResources.Address(),
                new IdentityResources.Email(),
                new IdentityResources.Phone(),
            };
        }

        public static IEnumerable<ApiScope> GetApiScopes()
        {
            return new ApiScope[]
            {
                    new ApiScope("Masters.Api", "Master Data"),
                    new ApiScope("theFinbalanceadminapi", "TheFinbalance admin API", new string[]
                    {
                        "name",
                        "role"
                    })
            };
        }

        public static IEnumerable<ApiResource> GetApis()
        {
            return new ApiResource[]
            {
                new ApiResource("Masters.Api", "Master Data"),
                new ApiResource("theFinbalanceadminapi", "TheFinbalance admin API", new string[]
                {
                    "name",
                    "role"
                })
                {
                    ApiSecrets = new List<Secret>
                    {
                        new Secret()
                        {
                            Type = SecretTypes.SharedSecret,
                            Value = "5b556f7c-b3bc-4b5b-85ab-45eed0cb962d".Sha256(),
                        }
                    }
                }

                };
        }

        public static IEnumerable<Client> GetClients()
        {
            return new[]
           {
            
            // m2m client credentials flow client
            new Client
            {
                ClientClaimsPrefix = null,
                ClientId = Guid.NewGuid().ToString(),
                ClientName = "Internal Clients",
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                ClientSecrets = { new Secret("511536EF-F270-4058-80CA-1C89C192F69A".Sha256()) },
                AllowedScopes = {"Masters.Api" }

            },

            // interactive client using code flow + pkce
            new Client
            {
                ClientClaimsPrefix = null,
                ClientId = Guid.NewGuid().ToString(),
                ClientSecrets = { new Secret("49C1A7E1-0C79-4A89-A3D6-A37998FB86B0".Sha256()) },
                ClientName = "FbSuperAdmin Services",
                AllowedGrantTypes = GrantTypes.HybridAndClientCredentials,
                RedirectUris = { "https://localhost:5441/signin-oidc" },
                FrontChannelLogoutUri = "https://localhost:5441/signout-oidc",
                PostLogoutRedirectUris = { "https://localhost:5441/signout-callback-oidc" },
                AllowOfflineAccess = true,
                AllowedScopes = { "openid", "profile", "Maters.Api" }
            },
             // react client  using code flow + pkce
            new Client
            {
                ClientClaimsPrefix = null,
                ClientId = Guid.NewGuid().ToString(),
                ClientName = "Application_User_Investor",
                ClientUri = "http://localhost:5002",
                AllowedGrantTypes = GrantTypes.Code,
                AllowedCorsOrigins = { "http://localhost:5002" },
                RequireClientSecret = false,
                RequirePkce = true,
                RedirectUris = { "http://localhost:5002" },
                PostLogoutRedirectUris = {"http://localhost:5002" },

                AllowOfflineAccess = true,
                AllowedScopes = { "openid", "profile","Masters.Api" }
            },

            new Client
            {
                ClientClaimsPrefix = null,
                ClientId = Guid.NewGuid().ToString(),
                ClientName = "FbSuperAdmin IT Infrastructure ",
                ClientUri = "https://localhost:5443/admin/",
                 AllowedGrantTypes = GrantTypes.Code,
                    RequirePkce = true,
                    RequireClientSecret = false,

                    RedirectUris =
                    {
                        "https://localhost:5443/admin/",
                        "http://exemple.com/"
                    },
                    PostLogoutRedirectUris =
                    {
                        "https://localhost:5443/admin/",
                        "http://exemple.com/"
                    },
                    AllowedCorsOrigins =
                    {
                        "https://localhost:5443",
                        "http://exemple.com/"
                    },
                    AllowedScopes = { "openid", "profile", "theFinbalanceadminapi" },
                    ClientSecrets = new List<Secret>
                    {
                        new Secret()
                        {
                            Type = SecretTypes.SharedSecret,
                            Value = "5b556f7c-b3bc-4b5b-85ab-45eed0cb962d".Sha256(),
                        }
                    }
            },
            new Client
            {
                    ClientClaimsPrefix = null,
                    ClientId = Guid.NewGuid().ToString(),
                    ClientName = "Public server Credentials Client",

                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    ClientSecrets = { new Secret("84137599-13d6-469c-9376-9e372dd2c1bd".Sha256()) },

                    AllowedScopes = { "theFinbalanceadminapi" },
                    Claims = new List<ClientClaim>
                    {
                        new ClientClaim("role", "Is4-Writer"),
                        new ClientClaim("role", "Is4-Reader")
                    },
                    FrontChannelLogoutSessionRequired = false,
                    BackChannelLogoutSessionRequired = false,
                    AccessTokenType = AccessTokenType.Reference
            },
         };
        }
    }
}
