using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.Core.IdentityEvents.Identity
{
    public class UserSavedEvent<TUserDto>
    {
        public TUserDto User { get; set; }

        public UserSavedEvent(TUserDto user)
        {
            User = user;
        }
    }
}
