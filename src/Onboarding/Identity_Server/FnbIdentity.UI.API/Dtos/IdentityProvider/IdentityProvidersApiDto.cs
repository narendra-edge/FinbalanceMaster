using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.UI.API.Dtos.IdentityProvider
{
    public class IdentityProvidersApiDto
    {
        public IdentityProvidersApiDto()
        {
            IdentityProviders = new List<IdentityProviderApiDto>();
        }

        public int PageSize { get; set; }

        public int TotalCount { get; set; }

        public List<IdentityProviderApiDto> IdentityProviders { get; set; }
    }
}
