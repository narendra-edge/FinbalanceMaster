using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.Core.Dtos.Keys
{
    public class KeyDto
    {
        public string? Id { get; set; }
        public int Version { get; set; }
        public DateTime Created { get; set; }
        public string? Use { get; set; }
        public string? Algorithm { get; set; }
        public bool IsX509Certificate { get; set; }
    }
}
