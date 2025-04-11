using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.Core.Dtos.Configuration
{
    public class ApiResourcePropertiesDto
    {
        public int ApiResourcePropertyId { get; set; }

        public int ApiResourceId { get; set; }

        public string? ApiResourceName { get; set; }

        [Required]
        public string? Key { get; set; }

        [Required]
        public string? Value { get; set; }

        public int TotalCount { get; set; }

        public int PageSize { get; set; }

        public List<ApiResourcePropertyDto> ApiResourceProperties { get; set; } = new();
    }
}
