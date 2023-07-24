using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masters.Infrastructure.Entities
{
    public  class DistrictMaster
    {
        [Key]
        public int DstrId { get; set; }
        public string DistrictCode { get; set; }
        public string DistrictName { get; set;}
        public int StateMId { get; set; }
        public StateMaster StateMaster { get; set; }       
        
        public ICollection<PincodeMaster> PincodeMaster { get; set;}
    }
}
