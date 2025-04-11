using FnbIdentity.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.Core.Resources
{
    public class KeyServiceResources : IKeyServiceResources
    {
        public ResourceMessage KeyDoesNotExist()
        {
            return new ResourceMessage()
            {
                Code = nameof(KeyDoesNotExist),
                Description = KeyServiceResource.KeyDoesNotExist
            };
        }
    }
}
