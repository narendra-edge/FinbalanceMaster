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
    public class KfinRawConfiguration : IEntityTypeConfiguration<KfinRaw>
    {
        public void Configure(EntityTypeBuilder<KfinRaw> builder)
        {
            builder.ToTable("kfin_raw");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            builder.Property(x => x.ImportedAt).HasColumnName("imported_at").HasDefaultValueSql("now()");
            builder.Property(x => x.SourceFile).HasColumnName("source_file");
            builder.Property(x => x.ProductCode).HasColumnName("\"Product Code\"");
            builder.Property(x => x.AmcCode).HasColumnName("\"Amc Code\"");
            builder.Property(x => x.AmcName).HasColumnName("\"Amc Name\"");
            builder.Property(x => x.SchemeCode).HasColumnName("\"Scheme Code\"");
            builder.Property(x => x.SchemeDescription).HasColumnName("\"Scheme Description\"");
            builder.Property(x => x.PlanCode).HasColumnName("\"Plan Code\"");
            builder.Property(x => x.PlanDescription).HasColumnName("\"Plan Description\"");
            builder.Property(x => x.OptionCode).HasColumnName("\"Option Code\"");
            builder.Property(x => x.OptionDescription).HasColumnName("\"Option Description\"");
            builder.Property(x => x.Nature).HasColumnName("\"Nature\"");
            builder.Property(x => x.FundType).HasColumnName("\"Fund Type\"");
            builder.Property(x => x.NfoStartDate).HasColumnName("\"Nfo Start Date\"");
            builder.Property(x => x.NfoEndDate).HasColumnName("\"Nfo End Date\"");
            builder.Property(x => x.OpenDate).HasColumnName("\"Open Date\"");
            builder.Property(x => x.CloseDate).HasColumnName("\"Close Date\"");
            builder.Property(x => x.IsinNumber).HasColumnName("\"Isin Number\"");
            builder.Property(x => x.IsinType).HasColumnName("\"Isin Type\"");
            builder.Property(x => x.PurchasedAllowed).HasColumnName("\"Purchased Allowed\"");
            builder.Property(x => x.IpoAmount).HasColumnName("\"Ipo Amount\"");
            builder.Property(x => x.IpoMinAmount).HasColumnName("\"Ipo Min Amount\"");
            builder.Property(x => x.IpoMultipleAmount).HasColumnName("\"Ipo Multiple Amount\"");
            builder.Property(x => x.NewPurchaseAmount).HasColumnName("\"New Purchase Amount\"");
            builder.Property(x => x.NewPurchaseMultipleAmount).HasColumnName("\"New Purchase Multiple Amount\"");
            builder.Property(x => x.NriNewMinAmount).HasColumnName("\"Nri New Min Amount\"");
            builder.Property(x => x.AddPurchaseAmount).HasColumnName("\"Add Purchase Amount\"");
            builder.Property(x => x.AddPurchaseMultipleAmount).HasColumnName("\"Add Purchase Multiple Amount\"");
            builder.Property(x => x.RedemptionAllowed).HasColumnName("\"Redemption Allowed\"");
            builder.Property(x => x.RedemptionMinAmount).HasColumnName("\"Redemption Min Amount\"");
            builder.Property(x => x.RedemptionMultipleAmount).HasColumnName("\"Redemption Multiple Amount\"");
            builder.Property(x => x.RedemptionMinUnits).HasColumnName("\"Redemption Min Units\"");
            builder.Property(x => x.RedemptionMultipleUnits).HasColumnName("\"Redemption Multiple Units\"");
            builder.Property(x => x.SwitchInAllowed).HasColumnName("\"Switch In Allowed\"");
            builder.Property(x => x.SwitchOutAllowed).HasColumnName("\"Switch Out Allowed\"");
            builder.Property(x => x.SwitchOutMinAmount).HasColumnName("\"Switch Out MinAmount\"");
            builder.Property(x => x.SwitchInMinAmount).HasColumnName("\"Switch In MinAmount\"");
            builder.Property(x => x.LateralInAllowed).HasColumnName("\"Lateral In Allowed\"");
            builder.Property(x => x.LateralOutAllowed).HasColumnName("\"Lateral Out Allowed\"");
            builder.Property(x => x.StpInAllowed).HasColumnName("\"Stp In Allowed\"");
            builder.Property(x => x.StpOutAllowed).HasColumnName("\"Stp Out Allowed\"");
            builder.Property(x => x.StpFrequency).HasColumnName("\"Stp Frequency\"");
            builder.Property(x => x.StpMinAmount).HasColumnName("\"Stp Min Amount\"");
            builder.Property(x => x.StpDates).HasColumnName("\"Stp Dates\"");
            builder.Property(x => x.SipAllowed).HasColumnName("\"Sip Allowed\"");
            builder.Property(x => x.SipMinAmount).HasColumnName("\"Sip Min Amount\"");
            builder.Property(x => x.SipDates).HasColumnName("\"Sip Dates\"");
            builder.Property(x => x.SipFrequency).HasColumnName("\"Sip Frequency\"");
            builder.Property(x => x.SwpInAllowed).HasColumnName("\"Swp In Allowed\"");
            builder.Property(x => x.SwpOutAllowed).HasColumnName("\"Swp Out Allowed\"");
            builder.Property(x => x.SwpFrequency).HasColumnName("\"Swp Frequency\"");
            builder.Property(x => x.SwpMinAmount).HasColumnName("\"Swp Min Amount\"");
            builder.Property(x => x.SwpDates).HasColumnName("\"Swp Dates\"");
            builder.Property(x => x.LoadDetails).HasColumnName("\"Load Details\"");
            builder.Property(x => x.PurchaseCutoffTime).HasColumnName("\"Purchase Cutoff Time\"");
            builder.Property(x => x.RedemptionCutoffTime).HasColumnName("\"Redemption Cutoff Time\"");
            builder.Property(x => x.SwitchCutoffTime).HasColumnName("\"Switch Cutoff Time\"");
            builder.Property(x => x.MaturityDate).HasColumnName("\"Maturity Date\"");
            builder.Property(x => x.ReOpenDate).HasColumnName("\"ReOpen Date\"");
            builder.Property(x => x.NfoFaceValue).HasColumnName("\"Nfo Face Value\"");
            builder.Property(x => x.DematAllowed).HasColumnName("\"Demat Allowed\"");
            builder.Property(x => x.RiskType).HasColumnName("\"Risk Type\"");
            builder.Property(x => x.AllotmentDate).HasColumnName("\"Allotment Date\"");
            builder.Property(x => x.LastUpdateDate).HasColumnName("\"Last Update Date\"");

        }
    }
}
