using FnbIdentity.Core.Shared.Configuration.Identity;
using FnbIdentity.STS.Identity.Configuration.Interfaces;

namespace FnbIdentity.STS.Identity.Configuration
{
    public class RootConfiguration : IRootConfiguration
    {
        public AdminConfiguration AdminConfiguration { get; } = new AdminConfiguration();
        public RegisterConfiguration RegisterConfiguration { get; } = new RegisterConfiguration();
    }
}
