using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masters.Infrastructure.Entities
{
    public class Occupation
    {
        [Key]
        public int OccupId { get; set; }
        public int BseOccupationCode { get; set; }
        public string OccupationName { get; set; }
        public string OccupationType { get; set; }
    }
}
