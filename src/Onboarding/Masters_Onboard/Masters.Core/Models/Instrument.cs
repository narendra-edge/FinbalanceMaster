using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Masters.Core.Models
{
    public  class Instrument
    {
        [JsonIgnore]
        public int InstId { get; set; }
        public string InstrumentName { get; set; }
        public string InstrumentType { get; set; }
        public string InstrumentIssuer { get; set; }
        public string Description { get; set; }
       
        public bool IsActive { get; set; }
        public string Risk { get; set; }
        public int ExchangeId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        
    }
}
