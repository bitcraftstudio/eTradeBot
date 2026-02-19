namespace TradeBotEngine.Core.Models;

/// <summary>
/// Stock quote data
/// </summary>
public class StockQuote
{
    public string Symbol { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal PreviousClose { get; set; }
    public decimal Change { get; set; }
    public decimal ChangePercent { get; set; }
    public decimal Open { get; set; }
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public long Volume { get; set; }
    public decimal? MarketCap { get; set; }
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Historical candle data
/// </summary>
public class CandleData
{
    public DateTime Date { get; set; }
    public decimal Open { get; set; }
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public decimal Close { get; set; }
    public long Volume { get; set; }
}

/// <summary>
/// Market data for persistence
/// </summary>
public class MarketData
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Symbol { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public decimal Open { get; set; }
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public decimal Close { get; set; }
    public long Volume { get; set; }
    
    // Cached indicators (optional)
    public decimal? RSI { get; set; }
    public decimal? MACDValue { get; set; }
    public decimal? MACDSignal { get; set; }
    public decimal? SMA20 { get; set; }
    public decimal? SMA50 { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// News article with sentiment
/// </summary>
public class NewsArticle
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Symbol { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime PublishedDate { get; set; }
    public string Source { get; set; } = string.Empty;
    public string? Url { get; set; }
    public string? Summary { get; set; }
    public string? Content { get; set; }
    
    // AI sentiment analysis
    public decimal Sentiment { get; set; } // -1 to 1
    public string? SentimentLabel { get; set; } // VERY_NEGATIVE to VERY_POSITIVE
    public decimal Relevance { get; set; } // 0 to 1
    public List<string> Keywords { get; set; } = new();
    
    // Trade linkage
    public string? LinkedTradeId { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Discovered stock from stock hunter
/// </summary>
public class DiscoveredStock
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Symbol { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    
    // TipRanks-style scores
    public int SmartScore { get; set; }
    public string Rating { get; set; } = string.Empty;
    public decimal CurrentPrice { get; set; }
    public decimal PriceTarget { get; set; }
    public decimal Upside { get; set; }
    
    // Analysis scores
    public AnalystConsensus? AnalystConsensus { get; set; }
    public string? HedgeFundTrend { get; set; }
    public string? InsiderSentiment { get; set; }
    public decimal NewsSentiment { get; set; }
    public int TechnicalScore { get; set; }
    public int FundamentalScore { get; set; }
    
    // Discovery details
    public DateTime DiscoveredAt { get; set; } = DateTime.UtcNow;
    public List<string> Reasons { get; set; } = new();
    
    // Status
    public bool AddedToWatchlist { get; set; }
    public bool TradeExecuted { get; set; }
    public string? TradeId { get; set; }
}

public class AnalystConsensus
{
    public decimal AverageRating { get; set; }
    public int StrongBuy { get; set; }
    public int Buy { get; set; }
    public int Hold { get; set; }
    public int Sell { get; set; }
    public int StrongSell { get; set; }
}

/// <summary>
/// Portfolio summary
/// </summary>
public class PortfolioSummary
{
    public decimal TotalValue { get; set; }
    public decimal CashBalance { get; set; }
    public decimal InvestedValue { get; set; }
    public decimal TotalPnL { get; set; }
    public decimal TotalPnLPercent { get; set; }
    public int OpenPositions { get; set; }
    public List<PositionSummary> Positions { get; set; } = new();
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

public class PositionSummary
{
    public string Symbol { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal AvgPrice { get; set; }
    public decimal CurrentPrice { get; set; }
    public decimal CurrentValue { get; set; }
    public decimal UnrealizedPnL { get; set; }
    public decimal UnrealizedPnLPercent { get; set; }
}
