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
    public class AmfiSchemeConfiguration : IEntityTypeConfiguration<AmfiScheme>
    {
        public void Configure(EntityTypeBuilder<AmfiScheme> builder)
        {
            builder.ToTable("amfi_scheme");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            builder.Property(x => x.ImportedAt).HasDefaultValueSql("now()");
            builder.Property(x => x.Code).HasColumnName("code");
            builder.Property(x => x.Amc).HasColumnName("amc");
            builder.Property(x => x.SchemeName).HasColumnName("scheme_name");
            builder.Property(x => x.Category).HasColumnName("category");
            builder.Property(x => x.SubCategory).HasColumnName("sub_category");
            builder.Property(x => x.SchemeNavName).HasColumnName("scheme_nav_name");
            builder.Property(x => x.NormalizeSchemeName).HasColumnName("normalize_scheme_name");
            builder.Property(x => x.IsinAll).HasColumnName("isin_all");
            builder.Property(x => x.IsinReinvestment).HasColumnName("isin_reinvestment");
            builder.Property(x => x.IsinDivPayout).HasColumnName("isin_div_payout");
            builder.Property(x => x.SchemeMinimumAmount).HasColumnName("scheme_min_amount");
            builder.Property(x => x.LaunchDate).HasColumnName("launch_date");
            builder.Property(x => x.ClosureDate).HasColumnName("closure_date");
            builder.Property(s => s.CreatedAt).HasColumnName("created_at");
            builder.Property(s => s.UpdatedAt).HasColumnName("updated_at");
        }
    }
}
