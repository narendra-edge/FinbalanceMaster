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
    public class SchemeMasterFinalConfiguration : IEntityTypeConfiguration<SchemeMasterFinal>
    {
        public void Configure(EntityTypeBuilder<SchemeMasterFinal> builder)
        {
            builder.ToTable("scheme_master_final");

            builder.HasKey(s => s.Id);
            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(s => s.RtaSource).HasColumnName("rta_source");
            builder.Property(s => s.RtaProductCode).HasColumnName("rta_product_code");
            builder.Property(s => s.RtaAmcCode).HasColumnName("rta_amc_code");
            builder.Property(s => s.RtaAmcName).HasColumnName("rta_amc_name");
            builder.Property(s => s.RtaSchemeCode).HasColumnName("rta_scheme_code");
            builder.Property(s => s.RtaSchemeName).HasColumnName("rta_scheme_name");
            builder.Property(s => s.RtaPlanCode).HasColumnName("rta_plan_code");
            builder.Property(s => s.RtaPlanName).HasColumnName("rta_plan_name");
            builder.Property(s => s.RtaNature).HasColumnName("rta_nature");
            builder.Property(s => s.RtaOptionCode).HasColumnName("rta_option_code");
            builder.Property(s => s.RtaOptionName).HasColumnName("rta_option_name");
            builder.Property(s => s.FaceValue).HasColumnName("face_value");
            builder.Property(s => s.DematAllow).HasColumnName("demat_allow");
            builder.Property(s => s.NewPurchaseFlag).HasColumnName("new_purchase_flag");
            builder.Property(s => s.RedemptionAllowed).HasColumnName("redemption_allowed");
            builder.Property(s => s.SipFlag).HasColumnName("sip_flag");
            builder.Property(s => s.StpInAllowed).HasColumnName("stp_in_allowed");
            builder.Property(s => s.StpOutAllowed).HasColumnName("stp_+out_allowed");
            builder.Property(s => s.SwpInAllowed).HasColumnName("swp_in_allowed");
            builder.Property(s => s.SwpOutAllowed).HasColumnName("swp_out_allowed");
            builder.Property(s => s.SwitchInAllowed).HasColumnName("switch_in_allowed");
            builder.Property(s => s.SwitchOutAllowed).HasColumnName("switch_out_allowed");
            builder.Property(s => s.LoadDetails).HasColumnName("load_details");
            builder.Property(s => s.Category).HasColumnName("category");
            builder.Property(s => s.SubCategory).HasColumnName("sub_category");
            builder.Property(s => s.Code).HasColumnName("code");
            builder.Property(s => s.SchemeName).HasColumnName("scheme_name");
            builder.Property(s => s.NormalizeSchemeName).HasColumnName("normalize_scheme_name");
            builder.Property(s => s.UnifiedIsin).HasColumnName("Unified_isin");
            builder.Property(s => s.MatchConfidence).HasColumnName("match_confidence");
            builder.Property(s => s.MappingSource).HasColumnName("mapping_source");
            builder.Property(s => s.VerifiedBy).HasColumnName("verified_by");
            builder.Property(s => s.VerifiedAt).HasColumnName("verified_at");
            builder.Property(s => s.Notes).HasColumnName("notes");
            builder.Property(s => s.CreatedAt).HasColumnName("created_at");
            builder.Property(s => s.UpdatedAt).HasColumnName("updated_at");
        }
    }
}
