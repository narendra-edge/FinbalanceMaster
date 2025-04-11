using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.Infrastructure.Entities
{
    public class DashboardDataView
    {
        public int ClientsTotal { get; set; }

        public int ApiResourcesTotal { get; set; }

        public int ApiScopesTotal { get; set; }

        public int IdentityResourcesTotal { get; set; }

        public int IdentityProvidersTotal { get; set; }
    }
}
