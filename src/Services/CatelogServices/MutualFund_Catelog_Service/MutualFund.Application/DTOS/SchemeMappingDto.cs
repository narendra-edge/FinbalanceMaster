using System;

namespace MutualFund.Application.DTOS
{
    public class SchemeMappingDto
    {
        public long Id { get; set; }
        public string? RtaSource { get; set; }
        public string? RtaProductCode { get; set; }
        public string? RtaAmcCode { get; set; }
        public string? RtaAmcName { get; set; }
        public string? RtaSchemeCode { get; set; }
        public string? RtaSchemeName { get; set; }
        public string? RtaPlanCode { get; set; }
        public string? RtaPlanName { get; set; }
        public string? RtaNature { get; set; }
        public string? RtaOptionCode { get; set; }
        public string? RtaOptionName { get; set; }
        public short? FaceValue { get; set; }
        public string? DematAllow { get; set; }
        public string? NewPurchaseFlag { get; set; }
        public string? RedemptionFlag { get; set; }
        public string? SipFlag { get; set; }
        public string? StpInAllowed { get; set; }
        public string? StpOutAllowed { get; set; }
        public string? SwpInAllowed { get; set; }
        public string? SwpOutAllowed { get; set; }
        public string? SwitchInAllowed { get; set; }
        public string? SwitchOutAllowed { get; set; }
        public string? LoadDetails { get; set; }
        public string? Code { get; set; }           // amfi code
        public string? SchemeName { get; set; }     // amfi scheme name
        public string? Category { get; set; }
        public string? SubCategory { get; set; }
        public string? Isin { get; set; }
        public string? NormalizeSchemeName { get; set; }
        public int? MatchConfidence { get; set; }
        public string? MappingSource { get; set; }
        public string? VerifiedBy { get; set; }
        public DateTime? VerifiedAt { get; set; }
    }
}
