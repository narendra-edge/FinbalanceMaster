using FnbIdentity.Core.IdentityDto.Identity.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.Core.IdentityDto.Identity.Base
{
    public class BaseUserProviderDto<TUserId> : IBaseUserProviderDto
    {
        public TUserId? UserId { get; set; }

        object IBaseUserProviderDto.UserId => UserId;
    }
}
