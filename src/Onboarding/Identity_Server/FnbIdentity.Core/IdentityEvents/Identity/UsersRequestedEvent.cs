using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.Core.IdentityEvents.Identity
{
    public class UsersRequestedEvent<TUsersDto>
    {
        public TUsersDto Users { get; set; }

        public UsersRequestedEvent(TUsersDto users)
        {
            Users = users;
        }
    }
}
