using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.Core.IdentityEvents.Identity
{
    public class UserPasswordChangedEvent
    {
        public string UserName { get; set; }

        public UserPasswordChangedEvent(string userName)
        {
            UserName = userName;
        }
    }
}
