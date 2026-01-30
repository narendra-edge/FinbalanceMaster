using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MutualFund.Core.Entities
{
    public class AmfiScheme
    {
        public Guid Id { get; set; }
        public DateTime ImportedAt { get; set; }
        public string? SourceFile { get; set; }

        public int? Code { get; set; }
        public string? Amc { get; set; }
        public string? SchemeName { get; set; }
        public string? SchemeType { get; set; }
        public string? Category { get; set; }
        public string? SubCategory { get; set; }
        public string? SchemeNavName { get; set; }
        public string? NormalizeSchemeName { get; set; }
        public string? IsinAll { get; set; }
        public string? IsinReinvestment { get; set; }
        public string? IsinDivPayout { get; set; }
        public decimal? SchemeMinimumAmount { get; set; }
        public DateTime? LaunchDate { get; set; }
        public DateTime? ClosureDate { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
