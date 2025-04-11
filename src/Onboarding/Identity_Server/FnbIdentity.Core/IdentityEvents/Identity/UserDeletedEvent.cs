using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.Core.IdentityEvents.Identity
{
    public class UserDeletedEvent<TUserDto>
    {
        public TUserDto User { get; set; }

        public UserDeletedEvent(TUserDto user)
        {
            User = user;
        }
    }
}
