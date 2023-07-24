using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masters.Infrastructure.Entities
{
    public class CountryCode
    {
        [Key]
        public int CtryId { get; set; }
        public string  CKyc_CountryCode { get; set; }
        public string Bse_CountryCode { get; set; }
        public string CountryName { get; set; }
        public ICollection<StateMaster> StateMaster { get; set; }

    }
}
