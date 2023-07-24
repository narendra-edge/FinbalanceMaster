using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masters.Core.Models
{
    public class SourceOfWealth
    {
        [JsonIgnore]
        public int SrcId { get; set; }
        public int BseSourceCode { get; set; }
        public string SourceName { get; set; }
    }
}
