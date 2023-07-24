using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Masters.Core.Models
{
    public class Occupation
    {
        [JsonIgnore]
        public int OccupId { get; set; }
        public int BseOccupationCode { get; set; }
        public string OccupationName { get; set; }
        public string OccupationType { get; set; }
    }
}
