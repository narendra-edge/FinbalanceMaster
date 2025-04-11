using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.Core.IdentityDto.Identity.Interfaces
{
    public interface IBaseRoleClaimDto
    {
        int ClaimId { get; set; }
        object RoleId { get; }
    }
}
