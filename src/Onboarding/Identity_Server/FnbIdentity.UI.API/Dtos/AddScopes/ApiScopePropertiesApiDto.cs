using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.UI.API.Dtos.AddScopes
{
    public class ApiScopePropertiesApiDto
    {
        public ApiScopePropertiesApiDto()
        {
            ApiScopeProperties = new List<ApiScopePropertyApiDto>();
        }

        public List<ApiScopePropertyApiDto> ApiScopeProperties { get; set; } = new List<ApiScopePropertyApiDto>();

        public int TotalCount { get; set; }

        public int PageSize { get; set; }
    }
}
