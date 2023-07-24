using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masters.Infrastructure.Entities
{
    public class TaxStatus
    {
        [Key]
        public int TaxId { get; set; }
        public string TaxCodeCkyc { get; set; }
        public string TaxStatusNameCkyc { get; set; }
        public int TaxCodeBse { get; set; }
        public string TaxStatusName { get; set; }
        public string TaxCateogary { get; set; }
        public string TaxPanValidation { get; set; }
       

    }
}
