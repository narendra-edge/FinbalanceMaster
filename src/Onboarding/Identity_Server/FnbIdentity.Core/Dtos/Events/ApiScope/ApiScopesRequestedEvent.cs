using FnbIdentity.Core.Dtos.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.Core.Dtos.Events.ApiScope
{
    public class ApiScopesRequestedEvent
    {
        public ApiScopesDto ApiScope { get; set; }

        public ApiScopesRequestedEvent(ApiScopesDto apiScope)
        {
            ApiScope = apiScope;
        }
    }
}
