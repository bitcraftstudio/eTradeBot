namespace TradeBotEngine.Core.Models;

/// <summary>
/// Represents a complete trade record with AI reasoning and context
/// </summary>
public class Trade
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string TradeId { get; set; } = string.Empty; // TRADE_2024_001 format
    public string Symbol { get; set; } = string.Empty;
    public TradeType Type { get; set; }
    
    // Execution details
    public int Quantity { get; set; }
    public decimal EntryPrice { get; set; }
    public decimal? ExitPrice { get; set; }
    public DateTime EntryDate { get; set; }
    public DateTime? ExitDate { get; set; }
    
    // Context at entry
    public ContextAtEntry ContextAtEntry { get; set; } = new();
    
    // AI Analysis
    public AIReasoning AIReasoning { get; set; } = new();
    
    // Risk Management
    public decimal StopLoss { get; set; }
    public decimal TakeProfit { get; set; }
    public decimal RiskRewardRatio { get; set; }
    public int PositionSize { get; set; }
    public decimal CapitalRisked { get; set; }
    
    // Outcome (when closed)
    public TradeOutcome? Outcome { get; set; }
    
    // Learning
    public List<string> LearningTags { get; set; } = new();
    public bool ReviewedByAI { get; set; }
    public DateTime? ReviewDate { get; set; }
    
    // Status
    public TradeStatus Status { get; set; } = TradeStatus.Open;
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // eTrade specific
    public string? ETradeOrderId { get; set; }
    public string? ETradeAccountId { get; set; }
}

public enum TradeType
{
    Buy,
    Sell
}

public enum TradeStatus
{
    Open,
    Closed,
    Cancelled,
    Pending
}

public class ContextAtEntry
{
    public TechnicalIndicators TechnicalIndicators { get; set; } = new();
    public List<NewsHeadline> NewsHeadlines { get; set; } = new();
    public MarketConditions MarketConditions { get; set; } = new();
}

public class TechnicalIndicators
{
    public decimal RSI { get; set; }
    public MACDData MACD { get; set; } = new();
    public BollingerBandsData BollingerBands { get; set; } = new();
    public decimal SMA20 { get; set; }
    public decimal SMA50 { get; set; }
    public decimal EMA12 { get; set; }
    public decimal EMA26 { get; set; }
    public long Volume { get; set; }
    public long VolumeAvg20 { get; set; }
}

public class MACDData
{
    public decimal Value { get; set; }
    public decimal Signal { get; set; }
    public decimal Histogram { get; set; }
}

public class BollingerBandsData
{
    public decimal Upper { get; set; }
    public decimal Middle { get; set; }
    public decimal Lower { get; set; }
}

public class NewsHeadline
{
    public string Title { get; set; } = string.Empty;
    public decimal Sentiment { get; set; } // -1 to 1
    public string Source { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string? Url { get; set; }
    public string? Summary { get; set; }
}

public class MarketConditions
{
    public decimal SPY { get; set; } // S&P 500 price
    public decimal VIX { get; set; } // Volatility index
    public string? SectorStrength { get; set; }
    public MarketTrend MarketTrend { get; set; }
}

public enum MarketTrend
{
    Bullish,
    Bearish,
    Neutral
}

public class AIReasoning
{
    public string Model { get; set; } = string.Empty; // e.g., "claude-3-sonnet", "gpt-4"
    public TradeDecision Decision { get; set; }
    public decimal Confidence { get; set; } // 0-1
    public string Reasoning { get; set; } = string.Empty;
    public RiskAssessment RiskAssessment { get; set; }
    public decimal ExpectedReturn { get; set; }
    public decimal MaxRisk { get; set; }
    public int TechnicalScore { get; set; }
    public decimal SentimentScore { get; set; }
    public int MomentumScore { get; set; }
}

public enum TradeDecision
{
    Buy,
    Sell,
    Hold
}

public enum RiskAssessment
{
    Low,
    Moderate,
    High,
    VeryHigh
}

public class TradeOutcome
{
    public decimal ProfitLoss { get; set; }
    public decimal ProfitLossPercent { get; set; }
    public int HoldingDays { get; set; }
    public ExitReason ExitReason { get; set; }
    public ActualVsExpected? ActualVsExpected { get; set; }
}

public enum ExitReason
{
    TakeProfitHit,
    StopLossHit,
    ManualExit,
    TimeExit,
    TrailingStop,
    ETradeRejected
}

public class ActualVsExpected
{
    public decimal ExpectedReturn { get; set; }
    public decimal ActualReturn { get; set; }
    public decimal AccuracyPercent { get; set; }
}
