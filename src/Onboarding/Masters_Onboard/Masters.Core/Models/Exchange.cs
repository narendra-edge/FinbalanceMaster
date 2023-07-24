using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Masters.Core.Models
{
    public class Exchange
    {
        [JsonIgnore]
        public int ExchId { get; set; }
        public string ExchangeName { get; set; }
        public string Description { get; set; }
        public ICollection<Instrument> Instrument { get; set; }
    }
}
