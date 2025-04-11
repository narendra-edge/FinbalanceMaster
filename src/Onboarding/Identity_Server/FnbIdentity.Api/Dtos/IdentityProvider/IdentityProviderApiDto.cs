using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FnbIdentity.Api.Dtos.IdentityProvider
{
    public class IdentityProviderApiDto
    {
        public IdentityProviderApiDto()
        {
        }

        public string Type { get; set; }

        public int Id { get; set; }

        [Required]
        public string Scheme { get; set; }

        public string DisplayName { get; set; }

        public bool Enabled { get; set; } = true;

        public Dictionary<string, string> IdentityProviderProperties { get; set; }
    }
}
