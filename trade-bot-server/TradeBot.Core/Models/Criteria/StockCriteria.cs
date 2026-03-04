using System;
using System.Collections.Generic;
using System.Text;

namespace TradeBot.Core.Models.Criteria
{
    public class StockCriteria
    {
        public long? MarketCapMin { get; set; }
        public long? MarketCapMax { get; set; }
        public List<string>? Sectors { get; set; }
        public decimal? PriceCapMin { get; set; }
        public decimal? PriceCapMax { get; set; }
        public bool? IsEtf { get; set; }
        public int? MinSmartScore { get; set; }
        public bool? IsExcluded { get; set; }
    }
}
