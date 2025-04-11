using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.Core.IdentityEvents.Identity
{
    public class UserUpdatedEvent<TUserDto>
    {
        public TUserDto OriginalUser { get; set; }
        public TUserDto User { get; set; }

        public UserUpdatedEvent(TUserDto originalUser, TUserDto user)
        {
            OriginalUser = originalUser;
            User = user;
        }
    }
}
