using System;
using System.Collections.Generic;
using System.Text;

namespace TradeBot.Core.Models.FMP
{
    public class RatingsSnapshot
    {
        public string Symbol { get; set; } = string.Empty;
        public string Rating { get; set; } = string.Empty;
        public int OverallScore { get; set; } = 0;
    }
}
