using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masters.Core.Models
{
    public class StateMaster
    {
        [JsonIgnore]
        public int StateId { get; set; }
        public string StateCode { get; set; }
        public string StateName { get; set; }
        public int CountryId { get; set; }

        public ICollection<DistrictMaster> DistrictMaster { get; set; }
    }
}
