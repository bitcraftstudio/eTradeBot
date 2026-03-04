using System;
using System.Collections.Generic;
using System.Text;
using TradeBot.Core.Models.FMP;

namespace TradeBot.Core.Models
{
    public class Stock
    {
        public string Id { get; set; } = string.Empty;
        public string Symbol { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal MarketCap { get; set; } = 0;
        public string Sector { get; set; } = string.Empty;
        public bool IsEtf { get; set; } = false;
        public string Exchange { get; set; } = string.Empty;
        public decimal Dividend { get; set; } = 0;
        public decimal Volume { get; set; } = 0;
        public decimal Price { get; set; } = 0;

        // Rank Scores
        public int? SmartScore { get; set; } = null;
        public string? Rating { get; set; } = string.Empty;
        public decimal? PriceTarget { get; set; } = null;
        public decimal? Upside { get; set; } = null;
        public string? Consensus { get; set; } = string.Empty;
        public double? ConsensusTrending { get; set; } = 0;

        // Analysis scores
        // public AnalystConsensus? AnalystConsensus { get; set; }
        //public string? HedgeFundTrend { get; set; } = null;
        //public string? InsiderSentiment { get; set; } = null;
        //public decimal? NewsSentiment { get; set; } = null;

        public BalanceSheet? BalanceSheet { get; set; } = null;
        public IncomeStatement? IncomeStatement { get; set; } = null;
        public CashFlowStatement? CashFlowStatement { get; set; } = null;
        public KeyMetrics? KeyMetrics { get; set; } = null;
        public StockGradeSummary? GradeSummary { get; set; } = null;
        public List<StockCandleStick>? CandleSticks { get; set; } = null;

        // Discovery details
        public DateTime DiscoveredAt { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        public List<string> Reasons { get; set; } = new();
        public bool IsExcluded { get; set; } = false;
    }
}
