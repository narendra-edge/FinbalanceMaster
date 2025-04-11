using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.Core.Dtos.IdentityProviders
{
    public class IdentityProvidersDto
    {
        public IdentityProvidersDto()
        {
            IdentityProviders = new List<IdentityProviderDto>();
        }

        public int PageSize { get; set; }

        public int TotalCount { get; set; }

        public List<IdentityProviderDto> IdentityProviders { get; set; }
    }
}
