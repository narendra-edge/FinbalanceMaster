using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.UI.API.Dtos.ApiResources
{
    public class ApiSecretApiDto
    {
        [Required]
        public string Type { get; set; } = "SharedSecret";

        public int Id { get; set; }

        public string? Description { get; set; }

        [Required]
        public string? Value { get; set; }

        public string? HashType { get; set; }

        public DateTime? Expiration { get; set; }
    }
}
