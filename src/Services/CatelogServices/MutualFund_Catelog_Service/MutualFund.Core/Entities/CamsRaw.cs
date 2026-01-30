using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MutualFund.Core.Entities
{
    public class CamsRaw
    {
        public Guid Id { get; set; }
        public DateTime ImportedAt { get; set; }
        public string? SourceFile { get; set; }
        public string? AmcCode { get; set; }
        public string? Amc { get; set; }
        public string? SchCode { get; set; }
        public string? SchName { get; set; }
        public string? SchType { get; set; }
        public string? DivReinv { get; set; }
        public bool? SipAllow { get; set; }
        public bool? Lien { get; set; }
        public decimal? SwtMnAmt { get; set; }
        public decimal? SwtMxAmt { get; set; }
        public decimal? SwtMnUnt { get; set; }
        public decimal? SwtMxUnt { get; set; }
        public decimal? SwiMulti { get; set; }
        public decimal? AdpMnAmt { get; set; }
        public decimal? AdpMxAmt { get; set; }
        public decimal? AdpMnUnt { get; set; }
        public decimal? AdpMxUnt { get; set; }
        public decimal? NewpMnval { get; set; }
        public decimal? NewpMxval { get; set; }
        public decimal? PMnIncr { get; set; }
        public decimal? RedMnAmt { get; set; }
        public decimal? RedMxAmt { get; set; }
        public decimal? RedMnUnt { get; set; }
        public decimal? RedMxUnt { get; set; }
        public decimal? RedIncr { get; set; }
        public decimal? MnSwpAmt { get; set; }
        public decimal? MxSwpAmt { get; set; }
        public bool? CloseEnd { get; set; }
        public bool? ElssSch { get; set; }
        public DateTime? MatureDt { get; set; }
        public decimal? FaceValue { get; set; }
        public string? AssetClass { get; set; }
        public string? SebiClass { get; set; }
        public decimal? SettlePer { get; set; }
        public short? SipDates { get; set; }
        public short? SwpDates { get; set; }
        public short? StpDates { get; set; }
        public string? SysFreqs { get; set; }
        public bool? SwpAllow { get; set; }
        public bool? StpAllow { get; set; }
        public string? PlanType { get; set; }
        public string? SfCode { get; set; }
        public string? SfName { get; set; }
        public DateTime? StartDate { get; set; }
        public bool? SwtAllow { get; set; }
        public decimal? AdpMnInc { get; set; }
        public bool? PurAllow { get; set; }
        public bool? RedAllow { get; set; }
        public decimal? SipMnIns { get; set; }
        public decimal? SipMnAmt { get; set; }
        public decimal? SipMulti { get; set; }
        public decimal? SipMxAmt { get; set; }
        public decimal? SwpMnIns { get; set; }
        public decimal? SwpMulti { get; set; }
        public string? ParentSchemeCode { get; set; }
        public string? IsinNo { get; set; }
        public string? DisplayDataEntry { get; set; }
        public DateTime? NfoEndDt { get; set; }
        public DateTime? OpenDate { get; set; }
        public DateTime? AllotmentDate { get; set; }
    }
}
