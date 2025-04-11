using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.Core.Dtos.Configuration
{
    public class ApiScopesDto
    {
        public ApiScopesDto()
        {
            Scopes = new List<ApiScopeDto>();
        }

        public int PageSize { get; set; }

        public int TotalCount { get; set; }

        public List<ApiScopeDto> Scopes { get; set; }
    }
}
