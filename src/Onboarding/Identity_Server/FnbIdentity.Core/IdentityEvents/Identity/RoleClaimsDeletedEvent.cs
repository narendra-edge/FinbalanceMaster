using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.Core.IdentityEvents.Identity
{
    public class RoleClaimsDeletedEvent<TRoleClaimsDto>
    {
        public TRoleClaimsDto RoleClaim { get; set; }

        public RoleClaimsDeletedEvent(TRoleClaimsDto roleClaim)
        {
            RoleClaim = roleClaim;
        }
    }
}
