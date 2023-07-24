using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masters.Infrastructure.Entities
{
    public  class Exchange
    {
        [Key]
        public int ExchId { get; set; }
        public string ExchangeName { get; set; }
        public string Description { get; set; }
        public ICollection<Instrument> Instrument { get; set; }
    }
}
