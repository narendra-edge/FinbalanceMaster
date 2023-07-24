using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Masters.Core.Models
{
    public class IncomeDetail
    {
        [JsonIgnore]
        public int IncmId { get; set; }
        public int IncomeCode { get; set; }
        public string IncomeRange { get; set; }
        
    }
}
