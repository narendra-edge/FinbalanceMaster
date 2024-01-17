using FnbIdentity.Infrastructure.Configuration.Identity;


namespace FnbIdentity.Infrastructure.Configuration.IdentityServer
{
    public class Client : global::Duende.IdentityServer.Models.Client
    {
        public List<Claim> ClientClaims {get; set;} = new List<Claim>();
    };
}
