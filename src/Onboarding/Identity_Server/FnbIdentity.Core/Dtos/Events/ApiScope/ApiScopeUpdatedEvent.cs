using FnbIdentity.Core.Dtos.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.Core.Dtos.Events.ApiScope
{
    public class ApiScopeUpdatedEvent
    {
        public ApiScopeDto OriginalApiScope { get; set; }
        public ApiScopeDto ApiScope { get; set; }

        public ApiScopeUpdatedEvent(ApiScopeDto originalApiScope, ApiScopeDto apiScope)
        {
            OriginalApiScope = originalApiScope;
            ApiScope = apiScope;
        }
    }
}
