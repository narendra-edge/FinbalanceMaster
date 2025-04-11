using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.Core.IdentityEvents.PersistedGrant
{
    public class PersistedGrantsIdentityDeletedEvent
    {
        public string UserId { get; set; }

        public PersistedGrantsIdentityDeletedEvent(string userId)
        {
            UserId = userId;
        }
    }
}
