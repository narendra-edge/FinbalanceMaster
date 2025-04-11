using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.Core.Dtos.Configuration
{
    public class ClientClaimDto
    {
        public int Id { get; set; }

        [Required]
        public string? Type { get; set; }

        [Required]
        public string? Value { get; set; }
    }
}
