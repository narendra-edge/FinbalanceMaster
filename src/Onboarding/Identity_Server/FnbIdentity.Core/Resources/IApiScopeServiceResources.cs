using FnbIdentity.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.Core.Resources
{
    public interface IApiScopeServiceResources
    {
        ResourceMessage ApiScopeDoesNotExist();
        ResourceMessage ApiScopeExistsValue();
        ResourceMessage ApiScopeExistsKey();
        ResourceMessage ApiScopePropertyExistsValue();
        ResourceMessage ApiScopePropertyDoesNotExist();
        ResourceMessage ApiScopePropertyExistsKey();
    }
}
