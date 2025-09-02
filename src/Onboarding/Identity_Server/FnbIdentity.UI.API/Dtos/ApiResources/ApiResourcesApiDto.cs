using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.UI.API.Dtos.ApiResources
{
    public class ApiResourcesApiDto
    {
        public ApiResourcesApiDto()
        {
            ApiResources = new List<ApiResourceApiDto>();
        }

        public int PageSize { get; set; }

        public int TotalCount { get; set; }

        public List<ApiResourceApiDto> ApiResources { get; set; }
    }
}
