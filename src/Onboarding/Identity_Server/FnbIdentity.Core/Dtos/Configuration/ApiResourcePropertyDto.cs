using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.Core.Dtos.Configuration
{
    public class ApiResourcePropertyDto
    {
        public int Id { get; set; }
        public string? Key { get; set; }
        public string? Value { get; set; }
    }
}
