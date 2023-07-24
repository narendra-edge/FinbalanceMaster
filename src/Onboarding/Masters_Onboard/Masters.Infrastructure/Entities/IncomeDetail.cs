using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masters.Infrastructure.Entities
{
    public  class IncomeDetail
    {
        [Key]
        public int IncmId { get; set; }
        public int IncomeCode { get; set; }
        public string IncomeRange { get; set; }
        
    }
}
