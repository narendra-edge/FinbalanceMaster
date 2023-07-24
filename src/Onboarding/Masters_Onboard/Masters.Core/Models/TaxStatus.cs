using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masters.Core.Models
{
    public class TaxStatus
    {
        [JsonIgnore]
        public int TaxId { get; set; }
        public string TaxCodeCkyc { get; set; }
        public string TaxStatusNameCkyc { get; set; }
        public int TaxCodeBse { get; set; }
        public string TaxStatusName { get; set; }
        public string TaxType { get; set; }
        public string TaxPanValidation { get; set; }
       
    }
}
