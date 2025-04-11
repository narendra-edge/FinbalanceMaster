using FnbIdentity.Core.Dtos.IdentityProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.Core.Dtos.Events.IdentityProvider
{
    public class IdentityProviderAddedEvent
    {
        public IdentityProviderDto IdentityProvider { get; set; }

        public IdentityProviderAddedEvent(IdentityProviderDto identityProvider)
        {
            IdentityProvider = identityProvider;
        }
    }
}
