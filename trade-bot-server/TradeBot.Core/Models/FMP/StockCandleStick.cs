using System;
using System.Collections.Generic;
using System.Text;

namespace TradeBot.Core.Models.FMP
{
    public class StockCandleStick
    {
        public string Symbol { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public long Volume { get; set; }
        public decimal Change { get; set; }
        public decimal ChangePercent { get; set; }
        public decimal VWAP { get; set; }
    }
}
