using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MutualFund.Application.DTOS
{
    public class SchemeMasterFinalDto
    {
        public Guid Id { get; set; }
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
        public string? FaceValue { get; set; }
        public string? DematAllow { get; set; }
        public string? NewPurchaseFlag { get; set; }
        public string? RedemptionAllowed { get; set; }
        public string? SipFlag { get; set; }
        public string? StpInAllowed { get; set; }
        public string? StpOutAllowed { get; set; }
        public string? SwpInAllowed { get; set; }
        public string? SwpOutAllowed { get; set; }
        public string? SwitchInAllowed { get; set; }
        public string? SwitchOutAllowed { get; set; }
        public string? LoadDetails { get; set; }
        public string? SchemeName { get; set; }
        public string? Category { get; set; }
        public string? SubCategory { get; set; }
        public string? UnifiedIsin { get; set; }
        public string? Code { get; set; }
        public string? NormalizeSchemeName { get; set; }
    }
}
