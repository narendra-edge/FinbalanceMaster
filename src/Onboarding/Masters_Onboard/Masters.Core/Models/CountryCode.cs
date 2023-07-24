using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Masters.Core.Models
{
    public class CountryCode
    {
        [JsonIgnore]
        public int CtryId { get; set; }
        public string CKyc_CountryCode { get; set; }
        public string Bse_CountryCode { get; set; }
        public string CountryName { get; set; }
        public ICollection<StateMaster> StateMaster { get; set; }
    }
}
