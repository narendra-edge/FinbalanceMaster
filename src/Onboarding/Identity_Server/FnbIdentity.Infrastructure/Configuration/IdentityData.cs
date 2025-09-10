using FnbIdentity.Infrastructure.Configuration.Identity;

namespace FnbIdentity.Infrastructure.Configuration
{
    public class IdentityData
    {
        public List<Role>? Roles { get; set; }
        public List<User>? Users { get; set; }
    }
}
