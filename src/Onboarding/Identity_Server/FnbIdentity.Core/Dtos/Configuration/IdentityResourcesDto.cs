using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.Core.Dtos.Configuration
{
    public class IdentityResourcesDto
    {
        public IdentityResourcesDto()
        {
            IdentityResources = new List<IdentityResourceDto>();
        }

        public int PageSize { get; set; }

        public int TotalCount { get; set; }

        public List<IdentityResourceDto> IdentityResources { get; set; }
    }
}
