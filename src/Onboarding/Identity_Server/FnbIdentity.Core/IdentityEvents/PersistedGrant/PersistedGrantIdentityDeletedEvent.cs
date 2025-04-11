using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.Core.IdentityEvents.PersistedGrant
{
    public class PersistedGrantIdentityDeletedEvent
    {
        public string Key { get; set; }

        public PersistedGrantIdentityDeletedEvent(string key)
        {
            Key = key;
        }
    }
}
