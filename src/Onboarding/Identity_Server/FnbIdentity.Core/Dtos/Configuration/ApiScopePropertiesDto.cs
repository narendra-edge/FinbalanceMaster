using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.Core.Dtos.Configuration
{
    public class ApiScopePropertiesDto
    {
        public int ApiScopePropertyId { get; set; }

        public int ApiScopeId { get; set; }

        public string? ApiScopeName { get; set; }

        [Required]
        public string? Key { get; set; }

        [Required]
        public string? Value { get; set; }

        public int TotalCount { get; set; }

        public int PageSize { get; set; }

        public List<ApiScopePropertyDto> ApiScopeProperties { get; set; } = new();
    }
}
