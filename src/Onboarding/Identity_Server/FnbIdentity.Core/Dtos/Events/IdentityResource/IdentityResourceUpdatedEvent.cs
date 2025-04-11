using FnbIdentity.Core.Dtos.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.Core.Dtos.Events.IdentityResource
{
    public class IdentityResourceUpdatedEvent
    {
        public IdentityResourceDto OriginalIdentityResource { get; set; }
        public IdentityResourceDto IdentityResource { get; set; }

        public IdentityResourceUpdatedEvent(IdentityResourceDto originalIdentityResource, IdentityResourceDto identityResource)
        {
            OriginalIdentityResource = originalIdentityResource;
            IdentityResource = identityResource;
        }
    }
}
