using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.Core.Dtos.Events.PersistantGrant
{
    public class PersistedGrantDeletedEvent
    {
        public string PersistedGrantKey { get; set; }

        public PersistedGrantDeletedEvent(string persistedGrantKey)
        {
            PersistedGrantKey = persistedGrantKey;
        }
    }
}
