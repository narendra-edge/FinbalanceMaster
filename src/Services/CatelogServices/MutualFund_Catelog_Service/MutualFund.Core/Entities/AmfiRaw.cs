using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MutualFund.Core.Entities
{
    public class AmfiRaw
    {
        public Guid Id { get; set; }
        public DateTime ImportedAt { get; set; }
        public string? SourceFile { get; set; }
        public string? Amc { get; set; }
        public string? Code { get; set; }
        public string? SchemeName { get; set; }
        public string? SchemeNavName { get; set; }
        public string? SchemeType { get; set; }
        public string? SchemeCategory { get; set; }
        public decimal? SchemeMinimumAmount { get; set; }
        public DateTime? LaunchDate { get; set; }
        public DateTime? ClosureDate { get; set; }
        public string? ISINDivPayoutISINGrowthISINDivReinvestment { get; set; }
        public string? ExtraJson { get; set; }    // JSONB stored as string
        public string? RawRow { get; set; }
    }
}
