using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masters.Infrastructure.Entities
{
    public  class Instrument
    {
        [Key]
        public int InstId { get; set; }
        [Required]
        public string InstrumentName { get; set; }
        [Required]
        public string InstrumentType { get; set; }
        [Required]
        public string InstrumentIssuer { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public bool IsActive { get; set; }
        [Required]
        public string Risk { get; set; }
        [Required]
        public int ExchangeId { get; set; }
        [Required]
        public string CreatedBy { get; set; }
        [Required]
        public DateTime CreatedDate { get; set; }

        public Exchange Exchange { get; set;}
        
    }
}
