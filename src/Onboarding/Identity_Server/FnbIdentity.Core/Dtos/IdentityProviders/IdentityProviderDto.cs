using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.Core.Dtos.IdentityProviders
{
    public class IdentityProviderDto
    {
        public IdentityProviderDto()
        {
        }

        [Required]
        public string Type { get; set; } = "oidc";

        public int Id { get; set; }

        [Required]
        public string Scheme { get; set; }

        public string DisplayName { get; set; }

        public bool Enabled { get; set; } = true;

        public Dictionary<int, IdentityProviderPropertyDto> Properties { get; set; } = new();
    }
}
