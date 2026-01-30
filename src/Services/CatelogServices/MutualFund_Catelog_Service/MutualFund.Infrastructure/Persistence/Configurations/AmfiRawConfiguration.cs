using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MutualFund.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MutualFund.Infrastructure.Persistence.Configurations
{
    public class AmfiRawConfiguration : IEntityTypeConfiguration<AmfiRaw>
    {
        public void Configure(EntityTypeBuilder<AmfiRaw> builder)
        {
            builder.ToTable("amfi_raw");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            builder.Property(x => x.ImportedAt).HasColumnName("imported_at").HasDefaultValueSql("now()");
            builder.Property(x => x.SourceFile).HasColumnName("source_file");
            builder.Property(x => x.Amc).HasColumnName("\"AMC\"");
            builder.Property(x => x.Code).HasColumnName("\"code\"");
            builder.Property(x => x.SchemeName).HasColumnName("\"Scheme Name\"");
            builder.Property(x => x.SchemeType).HasColumnName("\"Scheme Type\"");
            builder.Property(x => x.SchemeCategory).HasColumnName("\"Scheme Category\"");
            builder.Property(x => x.SchemeNavName).HasColumnName("\"Scheme Nav Name\"");
            builder.Property(x => x.SchemeMinimumAmount).HasColumnName("\"Scheme Minimum Amount\"");
            builder.Property(x => x.LaunchDate).HasColumnName("\"Launch Date\"");
            builder.Property(x => x.ClosureDate).HasColumnName("\"Closure Date\"");
            builder.Property(x => x.ISINDivPayoutISINGrowthISINDivReinvestment).HasColumnName("\"ISIN Div Payout/ISINGrowthISIN Div Reinvestment\"");
            builder.Property(x => x.RawRow).HasColumnName("raw_row");
        }
    }
}
