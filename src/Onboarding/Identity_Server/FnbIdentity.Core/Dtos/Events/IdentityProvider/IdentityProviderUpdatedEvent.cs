using FnbIdentity.Core.Dtos.IdentityProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.Core.Dtos.Events.IdentityProvider
{
    public class IdentityProviderUpdatedEvent
    {
        public IdentityProviderDto OriginalIdentityProvider { get; set; }
        public IdentityProviderDto IdentityProvider { get; set; }

        public IdentityProviderUpdatedEvent(IdentityProviderDto originalIdentityProvider, IdentityProviderDto identityProvider)
        {
            OriginalIdentityProvider = originalIdentityProvider;
            IdentityProvider = identityProvider;
        }
    }
}
