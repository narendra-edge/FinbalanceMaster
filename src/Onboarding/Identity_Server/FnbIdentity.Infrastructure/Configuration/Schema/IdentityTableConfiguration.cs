using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.Infrastructure.Configuration.Schema
{
    public class IdentityTableConfiguration
    {
        public string IdentityRoles { get; set; } = "Roles";
        public string IdentityRoleClaims { get; set; } = "RoleClaims";
        public string IdentityUserRoles { get; set; } = "UserRoles";
        public string IdentityUsers { get; set; } = "Users";
        public string IdentityUserLogins { get; set; } = "UserLogins";
        public string IdentityUserClaims { get; set; } = "UserClaims";
        public string IdentityUserTokens { get; set; } = "UserTokens";
    }
}
