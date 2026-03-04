using System;
using System.Collections.Generic;
using System.Text;

namespace TradeBot.Core.Models.FMP
{
    public class PriceTargetConsensus
    {
        public string Symbol { get; set; } = string.Empty;
        public decimal TargetConsensus { get; set; } = 0;
    }
}
