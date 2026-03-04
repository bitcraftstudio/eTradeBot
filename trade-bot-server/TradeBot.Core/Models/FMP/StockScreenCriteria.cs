using System;
using System.Collections.Generic;
using System.Text;

namespace TradeBot.Core.Models.FMP
{
    public class StockScreenCriteria
    {
        public decimal? MarketCapMoreThan { get; set; }
        public decimal? MarketCapLowerThan { get; set; }
        public string? Sector { get; set; }
        public string? Industry { get; set; }
        public decimal? BetaMoreThan { get; set; }
        public decimal? BetaLessThan { get; set; }
        public decimal? PriceMoreThan { get; set; }
        public decimal? PriceLowerThan { get; set; }
        public decimal? DividendMoreThan { get; set; }
        public decimal? DividendLowerThan { get; set; }
        public decimal? VolumeMoreThan { get; set; }
        public decimal? VolumeLowerThan { get; set; }
        public string? Exchange { get; set; }
        public string? Country { get; set; }
        public bool? IsEtf { get; set; } = false;
        public bool? IsFund { get; set; } = false;
        public bool? IsActivelyTrading { get; set; } = true;
        public int? Limit { get; set; }
        public bool? IncludeAllShareClasses { get; set; } = false;
    }
}
