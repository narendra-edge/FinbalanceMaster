using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.Core.IdentityEvents.Identity
{
    public class RoleAddedEvent<TRoleDto>
    {
        public TRoleDto Role { get; set; }

        public RoleAddedEvent(TRoleDto role)
        {
            Role = role;
        }
    }
}
