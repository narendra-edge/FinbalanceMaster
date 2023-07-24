using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masters.Infrastructure.Entities
{
    public class SourceOfWealth
    {
        [Key]
        public int SrcId { get; set; }
        public int BseSourceCode { get; set; }
        public string SourceName { get; set; }
    }
}

