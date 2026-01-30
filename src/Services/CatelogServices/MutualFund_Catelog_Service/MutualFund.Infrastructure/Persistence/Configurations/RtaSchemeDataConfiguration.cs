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
    public class RtaSchemeDataConfiguration: IEntityTypeConfiguration<RtaSchemeData>
    {
        public void Configure(EntityTypeBuilder<RtaSchemeData> builder)
        {
            builder.ToTable("rta_scheme_data");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            builder.Property(x => x.ImportedAt).HasDefaultValueSql("now()");
            builder.Property(x => x.SourceFile).HasMaxLength(255);
            builder.Property(x => x.RtaProductCode).HasColumnName("rta_product_code");
            builder.Property(x => x.RtaAmcCode).HasColumnName("rta_amc_code");
            builder.Property(x => x.RtaAmcName).HasColumnName("rta_amc_name");
            builder.Property(x => x.RtaSchemeCode).HasColumnName("rta_scheme_code");
            builder.Property(x => x.RtaSchemeName).HasColumnName("rta_scheme_name");
            builder.Property(x => x.RtaSubSchemeCode).HasColumnName("rta_sub_scheme_code");
            builder.Property(x => x.RtaSubSchemeName).HasColumnName("rta_sub_scheme_name");
            builder.Property(x => x.RtaPlanCode).HasColumnName("rta_plan_code");
            builder.Property(x => x.RtaPlanName).HasColumnName("rta_plan_name");
            builder.Property(x => x.RtaNature).HasColumnName("rta_nature");
            builder.Property(x => x.RtaOptionCode).HasColumnName("rta_option_code");
            builder.Property(x => x.RtaOptionName).HasColumnName("rta_option_name");
            builder.Property(x => x.IsinNo).HasColumnName("isin_no");
            builder.Property(x => x.RtaFundType).HasColumnName("rta_fund_type");
            builder.Property(x => x.IsinType).HasColumnName("isin_type");
            builder.Property(x => x.NfoStartDate).HasColumnName("nfo_start_date");
            builder.Property(x => x.NfoEndDate).HasColumnName("nfo_end_date");
            builder.Property(x => x.OpenDate).HasColumnName("open_date");
            builder.Property(x => x.MaturityDate).HasColumnName("maturity_date");
            builder.Property(x => x.AllotmentDate).HasColumnName("allotment_date");
            builder.Property(x => x.FaceValue).HasColumnName("face_value");
            builder.Property(x => x.DematAllow).HasColumnName("demat_allow");
            builder.Property(x => x.LoadDetails).HasColumnName("load_details");
            builder.Property(x => x.NewPurchaseFlag).HasColumnName("new_purchase_flag");
            builder.Property(x => x.NewPurchaseMinAmount).HasColumnName("new_purchase_min_amount");
            builder.Property(x => x.NewPurchaseMaxAmount).HasColumnName("new_purchase_max_amount");
            builder.Property(x => x.NewPurchaseMultipleAmount).HasColumnName("new_purchase_multiple_amount");
            builder.Property(x => x.PurchaseCutoffTime).HasColumnName("purchase_cutoff_time");
            builder.Property(x => x.AdditionalPurchaseAmount).HasColumnName("addl_purchase_amount");
            builder.Property(x => x.AdditionalPurchaseMaxAmount).HasColumnName("addl_purchase_max_amount");
            builder.Property(x => x.AdditionalPurchaseMinUnit).HasColumnName("addl_purchase_min_unit");
            builder.Property(x => x.AdditionalPurchaseMaxUnit).HasColumnName("addl_purchase_max_unit");
            builder.Property(x => x.AdditionalPurchaseMultipleAmount).HasColumnName("addl_purchase_multiple_amount");
            builder.Property(x => x.RedemptionFlag).HasColumnName("redemption_flag");
            builder.Property(x => x.RedemptionMinAmount).HasColumnName("redemption_min_amount");
            builder.Property(x => x.RedemptionMaxAmount).HasColumnName("redemption_max_amount");
            builder.Property(x => x.RedemptionMinUnit).HasColumnName("redemption_min_unit");
            builder.Property(x => x.RedemptionMaxUnit).HasColumnName("redemption_max_unit");
            builder.Property(x => x.RedemptionMultipleAmount).HasColumnName("redemption_multiple_amount");
            builder.Property(x => x.RedemptionMultipleUnit).HasColumnName("redemption_multiple_unit");
            builder.Property(x => x.RedemptionCutoffTime).HasColumnName("redemption_cutoff_time");
            builder.Property(x => x.SipFlag).HasColumnName("sip_flag");
            builder.Property(x => x.SipMinAmount).HasColumnName("sip_min_amount");
            builder.Property(x => x.SipMaxAmount).HasColumnName("sip_max_amount");
            builder.Property(x => x.SipFrequency).HasColumnName("sip_frequency");
            builder.Property(x => x.SipMinInstallment).HasColumnName("sip_min_installment");
            builder.Property(x => x.SipMultipleAmount).HasColumnName("sip_multiple_installment");
            builder.Property(x => x.SipDates).HasColumnName("sip_dates");
            builder.Property(x => x.StpInAllowed).HasColumnName("stp_in_allowed");
            builder.Property(x => x.StpOutAllowed).HasColumnName("stp_out_allowed");
            builder.Property(x => x.StpFrequency).HasColumnName("stp_frequency");
            builder.Property(x => x.StpDates).HasColumnName("stp_dates");
            builder.Property(x => x.StpMinAmount).HasColumnName("stp_min_amount");
            builder.Property(x => x.SwpInAllowed).HasColumnName("swp_in_allowed");
            builder.Property(x => x.SwpOutAllowed).HasColumnName("swp_out_allowed");
            builder.Property(x => x.SwpFrequency).HasColumnName("swp_frequency");
            builder.Property(x => x.SwpDates).HasColumnName("swp_dates");
            builder.Property(x => x.SwpMinAmount).HasColumnName("swp_min_amount");
            builder.Property(x => x.SwpMultipleAmount).HasColumnName("swp_multiple_amount");
            builder.Property(x => x.SwpMinInstallments).HasColumnName("swp_min_installments");
            builder.Property(x => x.SwitchInAllowed).HasColumnName("switch_in_allowed");
            builder.Property(x => x.SwitchInMinAmount).HasColumnName("switch_in_min_amount");
            builder.Property(x => x.SwitchOutMinAmount).HasColumnName("switch_out_min_amount");
            builder.Property(x => x.SwitchMaxAmount).HasColumnName("switch_max_amount");
            builder.Property(x => x.SwitchCutoffTime).HasColumnName("switch_cutoff_time");
            builder.Property(x => x.LateralInAllowed).HasColumnName("lateral_in_allowed");
            builder.Property(x => x.LateralOutAllowed).HasColumnName("lateral_out_allowed");
            builder.Property(x => x.ParentSchemeCode).HasColumnName("parent_scheme_code");
            builder.Property(x => x.DisplayDataEntry).HasColumnName("display_data_entry");
            builder.Property(x => x.RiskType).HasColumnName("risk_type");
            builder.Property(x => x.LastUpdateDate).HasColumnName("last_update_date");
            builder.Property(x => x.CloseDate).HasColumnName("close_date");
            builder.Property(x => x.IpoAmount).HasColumnName("ipo_amount");
            builder.Property(x => x.IpoMinAmount).HasColumnName("ipo_min_amount");
            builder.Property(x => x.IpoMultipleAmount).HasColumnName("ipo_multiple_amount");
            builder.Property(x => x.NormalizeSchemeName).HasColumnName("normalize_scheme_name");
            builder.Property(x => x.LienFlag).HasColumnName("lien_flag");
            
        }
    }
}
