using FnbIdentity.Core.Dtos.IdentityProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.Core.Dtos.Events.IdentityProvider
{
    public class IdentityProvidersRequestedEvent
    {

        public IdentityProvidersDto IdentityProviders { get; set; }

        public IdentityProvidersRequestedEvent(IdentityProvidersDto identityProviders)
        {
            IdentityProviders = identityProviders;
        }
    }
}
