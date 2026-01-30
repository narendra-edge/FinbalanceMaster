using System;

namespace MutualFund.Application.DTOS
{
    public class RtaSchemeDataDto
    {
        public Guid Id { get; set; }
        public DateTime? ImportedAt { get; set; }
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
        public string? NormalizeSchemeName { get; set; }
    }
}
