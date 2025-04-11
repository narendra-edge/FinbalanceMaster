using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.Core.Dtos.IdentityProviders
{
    public class IdentityProviderPropertyDto
    {
        [Required]
        public string? Name { get; set; }

        [Required]
        public string? Value { get; set; }
    }
}
