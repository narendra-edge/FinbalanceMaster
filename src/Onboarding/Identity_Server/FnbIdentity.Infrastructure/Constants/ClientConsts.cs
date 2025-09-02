using FnbIdentity.Infrastructure.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.Infrastructure.Constants
{
    public static class ClientConsts
    {
        public static List<string> GetSecretTypes()
        {
            var secretTypes = new List<string>
            {
                "SharedSecret",
                "X509Thumbprint",
                "X509Name",
                "X509CertificateBase64",
                "JWK"
            };
            return secretTypes;
        }

        public static List<string> GetStandardClaims()
        {
            var standardClaims = new List<string>
            {
                "name",
                "given_name",
                "family_name",
                "middle_name",
                "nickname",
                "preferred_username",
                "profile",
                "picture",
                "website",
                "gender",
                "birthdate",
                "zoneinfo",
                "locale",
                "address",
                "updated_at"
            };
            return standardClaims;
        }


        public static List<(string Id, string Label, bool Deprecated)> GetGrantTypes(bool includeObsoleteGrants)
        {
            var allowedGrantypes = new List<(string Id, string Label, bool Deprecated)>
            {
                ("authorization_code", "Authorization Code", false),
                ("implicit", "Implicit", true),
                ("client_credentials", "Client Credentials", false),
                ("hybrid", "Hybrid", false),
                ("password", "Password", true),
                ("urn:ietf:params:oauth:grant-type:device_code", "Device", false),
                ("delegation", "Delegation", false),
                ("urn:openid:params:grant-type:ciba", "CIBA", false)
            };

            return includeObsoleteGrants == false ?
                allowedGrantypes.Where(x => x.Deprecated == false).ToList() :
                allowedGrantypes;
        }

        public static List<string> SigningAlgorithms()
        {
            var signingAlgorithms = new List<string>
            {
                "RS256",
                "RS384",
                "RS512",
                "PS256",
                "PS384",
                "PS512",
                "ES256",
                "ES384",
                "ES512"
            };
            return signingAlgorithms;
        }

        public static List<SelectItem> GetProtocolTypes()
        {
            var protocolTypes = new List<SelectItem> { new ("oidc", "OpenID Connect") };
            return protocolTypes;
        }
    }
}
