using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MutualFund.Core.Entities
{
    public class KfinRaw
    {
        public Guid Id { get; set; }
        public DateTime ImportedAt { get; set; }
        public string? SourceFile { get; set; }
        public string? ProductCode { get; set; }
        public string? AmcCode { get; set; }
        public string? AmcName { get; set; }
        public string? SchemeCode { get; set; }
        public string? SchemeDescription { get; set; }
        public string? PlanCode { get; set; }
        public string? PlanDescription { get; set; }
        public string? OptionCode { get; set; }
        public string? OptionDescription { get; set; }
        public string? Nature { get; set; }
        public string? FundType { get; set; }
        public DateTime? NfoStartDate { get; set; }
        public DateTime? NfoEndDate { get; set; }
        public DateTime? OpenDate { get; set; }
        public DateTime? CloseDate { get; set; }
        public string? IsinNumber { get; set; }
        public decimal? IsinType { get; set; }
        public bool? PurchasedAllowed { get; set; }
        public decimal? IpoAmount { get; set; }
        public decimal? IpoMinAmount { get; set; }
        public decimal? IpoMultipleAmount { get; set; }
        public decimal? NewPurchaseAmount { get; set; }
        public decimal? NewPurchaseMultipleAmount { get; set; }
        public decimal? NriNewMinAmount { get; set; }
        public decimal? AddPurchaseAmount { get; set; }
        public decimal? AddPurchaseMultipleAmount { get; set; }
        public bool? RedemptionAllowed { get; set; }
        public decimal? RedemptionMinAmount { get; set; }
        public decimal? RedemptionMultipleAmount { get; set; }
        public decimal? RedemptionMinUnits { get; set; }
        public decimal? RedemptionMultipleUnits { get; set; }
        public bool? SwitchInAllowed { get; set; }
        public bool? SwitchOutAllowed { get; set; }
        public decimal? SwitchOutMinAmount { get; set; }
        public decimal? SwitchInMinAmount { get; set; }
        public bool? LateralInAllowed { get; set; }
        public bool? LateralOutAllowed { get; set; }
        public bool? StpInAllowed { get; set; }
        public bool? StpOutAllowed { get; set; }
        public string? StpFrequency { get; set; }
        public decimal? StpMinAmount { get; set; }
        public short? StpDates { get; set; }
        public bool? SipAllowed { get; set; }
        public decimal? SipMinAmount { get; set; }
        public short? SipDates { get; set; }
        public string? SipFrequency { get; set; }
        public bool? SwpInAllowed { get; set; }
        public bool? SwpOutAllowed { get; set; }
        public string? SwpFrequency { get; set; }
        public decimal? SwpMinAmount { get; set; }
        public DateTime? SwpDates { get; set; }
        public string? LoadDetails { get; set; }
        public TimeSpan? PurchaseCutoffTime { get; set; }
        public TimeSpan? RedemptionCutoffTime { get; set; }
        public TimeSpan? SwitchCutoffTime { get; set; }
        public DateTime? MaturityDate { get; set; }
        public DateTime? ReOpenDate { get; set; }
        public decimal? NfoFaceValue { get; set; }
        public bool? DematAllowed { get; set; }
        public string? RiskType { get; set; }
        public DateTime? AllotmentDate { get; set; }
        public DateTime? LastUpdateDate { get; set; }
    }
}
