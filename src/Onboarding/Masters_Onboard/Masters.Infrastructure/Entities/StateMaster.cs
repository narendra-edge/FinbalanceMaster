using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masters.Infrastructure.Entities
{
    public class StateMaster
    {
        [Key]
        public int StateId { get; set; }
        public string StateCode { get; set; }
        public string StateName { get; set; }
        public int CountryId { get; set; }
        public CountryCode CountryCode { get; set; }

        public ICollection<DistrictMaster> DistrictMaster { get; set; }
       
    }

}
