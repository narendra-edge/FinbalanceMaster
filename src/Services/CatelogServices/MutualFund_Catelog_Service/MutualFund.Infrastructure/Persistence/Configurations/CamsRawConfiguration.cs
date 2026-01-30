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
    public class CamsRawConfiguration : IEntityTypeConfiguration<CamsRaw>
    {
        public void Configure(EntityTypeBuilder<CamsRaw> builder)
        {
            builder.ToTable("cams_raw");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            builder.Property(x => x.ImportedAt).HasColumnName("imported_at").HasDefaultValueSql("now()");
            builder.Property(x => x.SourceFile).HasColumnName("source_file");
            builder.Property(x => x.AmcCode).HasColumnName("amc_code");
            builder.Property(x => x.Amc).HasColumnName("amc");
            builder.Property(x => x.SchCode).HasColumnName("sch_code");
            builder.Property(x => x.SchName).HasColumnName("sch_name");
            builder.Property(x => x.SchType).HasColumnName("sch_type");
            builder.Property(x => x.DivReinv).HasColumnName("div_reniv"); 
            builder.Property(x => x.SipAllow).HasColumnName("sip_allow");      
            builder.Property(x => x.Lien).HasColumnName("lien");
            builder.Property(x => x.SwtMnAmt).HasColumnName("swt_mn_amt");
            builder.Property(x => x.SwtMxAmt).HasColumnName("swt_mx_amt");
            builder.Property(x => x.SwtMnUnt).HasColumnName("swt_mn_unt");
            builder.Property(x => x.SwtMxUnt).HasColumnName("swt_mx_unt");
            builder.Property(x => x.SwiMulti).HasColumnName("swi_multi");
            builder.Property(x => x.AdpMnAmt).HasColumnName("adp_mn_amt");
            builder.Property(x => x.AdpMxAmt).HasColumnName("adp_mx_amt");
            builder.Property(x => x.AdpMnUnt).HasColumnName("adp_mn_unt");
            builder.Property(x => x.AdpMxUnt).HasColumnName("adp_mx_unt");
            builder.Property(x => x.NewpMnval).HasColumnName("newp_mn_val");
            builder.Property(x => x.NewpMxval).HasColumnName("newp_mx_val");
            builder.Property(x => x.PMnIncr).HasColumnName("p_mn_incr");
            builder.Property(x => x.RedMnAmt).HasColumnName("red_mn_amt");
            builder.Property(x => x.RedMxAmt).HasColumnName("red_mx_amt");
            builder.Property(x => x.RedMnUnt).HasColumnName("red_mn_unt");
            builder.Property(x => x.RedMxUnt).HasColumnName("red_mx_unt");
            builder.Property(x => x.RedIncr).HasColumnName("red_incr");
            builder.Property(x => x.MnSwpAmt).HasColumnName("mn_swp_amt");
            builder.Property(x => x.MxSwpAmt).HasColumnName("mx_swp_amt");
            builder.Property(x => x.CloseEnd).HasColumnName("close_end");
            builder.Property(x => x.ElssSch).HasColumnName("elss_sch");
            builder.Property(x => x.MatureDt).HasColumnName("mature_dt");
            builder.Property(x => x.FaceValue).HasColumnName("face_value");
            builder.Property(x => x.AssetClass).HasColumnName("asset_class");
            builder.Property(x => x.SebiClass).HasColumnName("sebi_class");
            builder.Property(x => x.SettlePer).HasColumnName("settle_per");
            builder.Property(x => x.SipDates).HasColumnName("sip_dates");
            builder.Property(x => x.SwpDates).HasColumnName("swp_dates");
            builder.Property(x => x.StpDates).HasColumnName("stp_dates");
            builder.Property(x => x.SysFreqs).HasColumnName("sys_freqs");
            builder.Property(x => x.SwpAllow).HasColumnName("swp_allow");
            builder.Property(x => x.StpAllow).HasColumnName("stp_allow");
            builder.Property(x => x.PlanType).HasColumnName("plan_type");
            builder.Property(x => x.SfCode).HasColumnName("sf_code");
            builder.Property(x => x.SfName).HasColumnName("sf_name");
            builder.Property(x => x.StartDate).HasColumnName("start_date");
            builder.Property(x => x.SwtAllow).HasColumnName("swt_allow");
            builder.Property(x => x.AdpMnInc).HasColumnName("adp_mn_inc");
            builder.Property(x => x.PurAllow).HasColumnName("pur_allow");
            builder.Property(x => x.RedAllow).HasColumnName("red_allow");
            builder.Property(x => x.SipMnIns).HasColumnName("sip_mn_ins");
            builder.Property(x => x.SipMnAmt).HasColumnName("sip_mn_amt");
            builder.Property(x => x.SipMulti).HasColumnName("sip_multi");
            builder.Property(x => x.SipMxAmt).HasColumnName("sip_mx_amt");
            builder.Property(x => x.SwpMnIns).HasColumnName("swp_mn_ins");
            builder.Property(x => x.SwpMulti).HasColumnName("swp_multi");
            builder.Property(x => x.ParentSchemeCode).HasColumnName("parent_scheme_code");
            builder.Property(x => x.IsinNo).HasColumnName("isin_no");
            builder.Property(x => x.DisplayDataEntry).HasColumnName("display_data_entry");
            builder.Property(x => x.NfoEndDt).HasColumnName("nfo_end_dt");
            builder.Property(x => x.OpenDate).HasColumnName("open_date");
            builder.Property(x => x.AllotmentDate).HasColumnName("allotment_date");

        }
    }
}
