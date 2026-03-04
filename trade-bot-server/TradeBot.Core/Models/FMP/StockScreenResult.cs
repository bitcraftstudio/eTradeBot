using System;
using System.Collections.Generic;
using System.Text;

namespace TradeBot.Core.Models.FMP
{
    public class StockScreenResult
    {
        public string Symbol { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public long MarketCap { get; set; }
        public string Sector { get; set; } = string.Empty;
        public string Industry { get; set; } = string.Empty;
        public decimal? Beta { get; set; }
        public decimal Price { get; set; }
        public decimal? LastAnnualDividend { get; set; }
        public decimal Volume { get; set; }
        public string Exchange { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;   
        public bool? IsEtf { get; set; }
        public bool? IsFund { get; set; }
    }
}
