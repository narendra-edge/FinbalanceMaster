using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.Core.IdentityEvents.Identity
{
    public class RoleDeletedEvent<TRoleDto>
    {
        public TRoleDto Role { get; set; }

        public RoleDeletedEvent(TRoleDto role)
        {
            Role = role;
        }
    }
}
