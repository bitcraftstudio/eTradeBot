using System;
using System.Collections.Generic;
using System.Text;

namespace TradeBot.Core.Models.FMP
{
    public class StockGrade
    {
        public string Symbol { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string GradingCompany { get; set; } = string.Empty;
        public string PreviousGrade { get; set; } = string.Empty;
        public string NewGrade { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
    }
}
