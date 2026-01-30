using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MutualFund.Core.Entities
{
    public class RtaSchemeData
    {
        public Guid Id { get; set; }
        public DateTime ImportedAt { get; set; }
        public string? SourceFile { get; set; }

        public string? RtaProductCode { get; set; }
        public string? RtaAmcCode { get; set; }
        public string? RtaAmcName { get; set; }
        public string? RtaSchemeCode { get; set; }
        public string? RtaSchemeName { get; set; }
        public string? RtaSubSchemeCode { get; set; }
        public string? RtaSubSchemeName { get; set; }
        public string? RtaPlanCode { get; set; }
        public string? RtaPlanName { get; set; }
        public string? RtaNature { get; set; }
        public string? RtaOptionCode { get; set; }
        public string? RtaOptionName { get; set; }
        public string? IsinNo { get; set; }
        public string? RtaFundType { get; set; }
        public string? IsinType { get; set; }
        public DateTime? NfoStartDate { get; set; }
        public DateTime? NfoEndDate { get; set; }
        public DateTime? OpenDate { get; set; }
        public DateTime? MaturityDate { get; set; }
        public DateTime? AllotmentDate { get; set; }
        public decimal? FaceValue { get; set; }
        public bool? DematAllow { get; set; }
        public string? LoadDetails { get; set; }
        public bool? NewPurchaseFlag { get; set; }
        public decimal? NewPurchaseMinAmount { get; set; }
        public decimal? NewPurchaseMaxAmount { get; set; }
        public decimal? NewPurchaseMultipleAmount { get; set; }
        public TimeOnly? PurchaseCutoffTime { get; set; }
        public decimal? AdditionalPurchaseAmount { get; set; }
        public decimal? AdditionalPurchaseMaxAmount { get; set; }
        public decimal? AdditionalPurchaseMinUnit { get; set; }
        public decimal? AdditionalPurchaseMaxUnit { get; set; }
        public decimal? AdditionalPurchaseMultipleAmount { get; set; }
        public bool? RedemptionFlag { get; set; }
        public decimal? RedemptionMinAmount { get; set; }
        public decimal? RedemptionMaxAmount { get; set; }
        public decimal? RedemptionMinUnit { get; set; }
        public decimal? RedemptionMaxUnit { get; set; }
        public decimal? RedemptionMultipleAmount { get; set; }
        public decimal? RedemptionMultipleUnit { get; set; }
        public TimeOnly? RedemptionCutoffTime { get; set; }
        public bool? SipFlag { get; set; }
        public decimal? SipMinAmount { get; set; }
        public decimal? SipMaxAmount { get; set; }
        public string? SipFrequency { get; set; }
        public decimal? SipMinInstallment { get; set; }
        public decimal? SipMultipleAmount { get; set; }
        public int? SipDates { get; set; }
        public bool? StpInAllowed { get; set; }
        public bool? StpOutAllowed { get; set; }
        public string? StpFrequency { get; set; }
        public int? StpDates { get; set; }
        public decimal? StpMinAmount { get; set; }
        public bool? SwpInAllowed { get; set; }
        public bool? SwpOutAllowed { get; set; }
        public string? SwpFrequency { get; set; }
        public int? SwpDates { get; set; }
        public decimal? SwpMinAmount { get; set; }
        public decimal? SwpMultipleAmount { get; set; }
        public decimal? SwpMinInstallments { get; set; }
        public bool? SwitchInAllowed { get; set; }
        public bool? SwitchOutAllowed { get; set; }
        public decimal? SwitchInMinAmount { get; set; }
        public decimal? SwitchOutMinAmount { get; set; }
        public decimal? SwitchMaxAmount { get; set; }
        public TimeOnly? SwitchCutoffTime { get; set; }
        public bool? LateralInAllowed { get; set; }
        public bool? LateralOutAllowed { get; set; }
        public string? ParentSchemeCode { get; set; }
        public string? DisplayDataEntry { get; set; }
        public string? RiskType { get; set; }
        public DateTime? LastUpdateDate { get; set; }
        public DateTime? CloseDate { get; set; }
        public decimal? IpoAmount { get; set; }
        public decimal? IpoMinAmount { get; set; }
        public decimal? IpoMultipleAmount { get; set; }
        public string? NormalizeSchemeName { get; set; }
        public bool? LienFlag { get; set; }
    }
}
