using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masters.Infrastructure.Entities
{
    public class PincodeMaster
    {
        [Key]
        public int PinId { get; set; }
        public int Pincode { get; set; }
        public string CityName { get; set; }
        public string Locality { get; set; }
        public int DistrictId { get; set; }     
        public DistrictMaster DistrictMaster { get; set; }
        
        
    }
}
