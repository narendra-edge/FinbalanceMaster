using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.Core.Dtos.Configuration
{
    public class IdentityResourcePropertiesDto
    {
        public int IdentityResourcePropertyId { get; set; }

        public int IdentityResourceId { get; set; }

        public string? IdentityResourceName { get; set; }

        [Required]
        public string? Key { get; set; }

        [Required]
        public string? Value { get; set; }

        public int TotalCount { get; set; }

        public int PageSize { get; set; }

        public List<IdentityResourcePropertyDto> IdentityResourceProperties { get; set; } = new();
    }
}
