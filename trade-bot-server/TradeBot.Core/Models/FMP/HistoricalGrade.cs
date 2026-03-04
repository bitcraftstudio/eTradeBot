using System;
using System.Collections.Generic;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TradeBot.Core.Models.FMP
{
    public class HistoricalGrade
    {
        public string Symbol { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public int AnalystRatingsStrongBuy { get; set; } = 0;
        public int AnalystRatingsBuy { get; set; } = 0;
        public int AnalystRatingsHold { get; set; } = 0;
        public int AnalystRatingsSell { get; set; } = 0;
        public int AnalystRatingsStrongSell { get; set; } = 0;
    }
}
