using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masters.Core.Models
{
    public class UBOCode
    {
        [JsonIgnore]
        public int UboId { get; set; }
        public string UboCode { get; set; }
        public string UboDetail { get; set; }
    }
}
