using System;
using System.Collections.Generic;
using System.Text;

namespace TradeBot.Core.Models.FMP
{
    public class StockGradeSummary
    {
        public string Symbol { get; set; } = string.Empty;
        public int StrongBuy { get; set; } = 0;
        public int Buy { get; set; } = 0;
        public int Hold { get; set; } = 0;
        public int Sell { get; set; } = 0;
        public int StrongSell { get; set; } = 0;
        public string Consensus { get; set; } = string.Empty;
        public IEnumerable<HistoricalGrade> HistoricalEntries { get; set; } = new List<HistoricalGrade>();
    }
}
