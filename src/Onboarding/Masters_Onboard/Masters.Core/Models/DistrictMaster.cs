using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Masters.Core.Models
{
    public  class DistrictMaster
    {
        [JsonIgnore]
        public int DstrId { get; set; }
        public string DistrictCode { get; set; }
        public string DistrictName { get; set; }
        public int StateMId { get; set; }

        public ICollection<PincodeMaster> PincodeMaster { get; set; }

    }
}
