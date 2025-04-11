using FnbIdentity.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.Core.Resources
{
    public class IdentityProviderServiceResources : IIdentityProviderServiceResources
    {
        public virtual ResourceMessage IdentityProviderDoesNotExist()
        {
            return new ResourceMessage()
            {
                Code = nameof(IdentityProviderDoesNotExist),
                Description = IdentityProviderServiceResource.IdentityProviderDoesNotExist
            };
        }

        public virtual ResourceMessage IdentityProviderExistsKey()
        {
            return new ResourceMessage()
            {
                Code = nameof(IdentityProviderExistsKey),
                Description = IdentityProviderServiceResource.IdentityProviderExistsKey
            };
        }

        public virtual ResourceMessage IdentityProviderExistsValue()
        {
            return new ResourceMessage()
            {
                Code = nameof(IdentityProviderExistsValue),
                Description = IdentityProviderServiceResource.IdentityProviderExistsValue
            };
        }
    }
}
