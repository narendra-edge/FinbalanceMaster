using FnbIdentity.Core.IdentityDto.Identity.Base;
using FnbIdentity.Core.IdentityDto.Identity.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.Core.IdentityDto.Identity
{
    public class RoleDto<TKey> : BaseRoleDto<TKey>, IRoleDto
    {
        [Required]
        public string Name { get; set; }
    }
}
