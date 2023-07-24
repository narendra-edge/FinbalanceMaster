using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masters.Infrastructure.Entities
{
    public class UBOCode
    {
        [Key]
        public int UboId { get; set; }
        public string UboCode { get; set; }
        public string UboDetail { get; set; }
    }
}
