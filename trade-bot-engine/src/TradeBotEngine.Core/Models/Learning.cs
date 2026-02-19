namespace TradeBotEngine.Core.Models;

/// <summary>
/// Learning insight from daily AI review
/// </summary>
public class LearningInsight
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime Date { get; set; }
    
    // Review summary
    public int TradesReviewed { get; set; }
    public int SuccessfulTrades { get; set; }
    public int FailedTrades { get; set; }
    public decimal TotalProfitLoss { get; set; }
    public decimal AvgReturn { get; set; }
    public decimal WinRate { get; set; }
    public decimal AvgHoldingDays { get; set; }
    
    // Discovered patterns
    public List<PatternInsight> Patterns { get; set; } = new();
    
    // AI analysis
    public List<string> SuccessFactors { get; set; } = new();
    public List<string> FailureFactors { get; set; } = new();
    public List<string> Adjustments { get; set; } = new();
    public string AIReflection { get; set; } = string.Empty;
    public string AIModel { get; set; } = string.Empty;
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class PatternInsight
{
    public string Pattern { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Occurrences { get; set; }
    public decimal SuccessRate { get; set; }
    public decimal AvgReturn { get; set; }
    public PatternConfidence Confidence { get; set; }
    public List<PatternExample> Examples { get; set; } = new();
}

public enum PatternConfidence
{
    Low,
    Medium,
    High
}

public class PatternExample
{
    public string TradeId { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public string Outcome { get; set; } = string.Empty;
    public decimal Return { get; set; }
}

/// <summary>
/// Strategy weights that evolve based on learning
/// </summary>
public class StrategyWeights
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    public string RiskProfile { get; set; } = string.Empty;
    
    // Indicator weights (0-1)
    public IndicatorWeights Indicators { get; set; } = new();
    
    // Entry criteria adjustments
    public EntryCriteria EntryCriteria { get; set; } = new();
    
    // Exit criteria adjustments
    public ExitCriteria ExitCriteria { get; set; } = new();
    
    // Performance tracking
    public int TotalTradesAnalyzed { get; set; }
    public decimal OverallWinRate { get; set; }
    public decimal OverallAvgReturn { get; set; }
}

public class IndicatorWeights
{
    public decimal RSI { get; set; } = 0.25m;
    public decimal MACD { get; set; } = 0.25m;
    public decimal BollingerBands { get; set; } = 0.15m;
    public decimal MovingAverages { get; set; } = 0.20m;
    public decimal Volume { get; set; } = 0.15m;
}

public class EntryCriteria
{
    public decimal MinConfidence { get; set; } = 0.70m;
    public decimal RSIOversoldThreshold { get; set; } = 30m;
    public decimal RSIOverboughtThreshold { get; set; } = 70m;
    public decimal MinVolumeRatio { get; set; } = 1.0m;
    public bool RequireMAAlignment { get; set; } = false;
}

public class ExitCriteria
{
    public decimal ProfitTargetMultiplier { get; set; } = 1.0m;
    public decimal TrailingStopActivation { get; set; } = 0.02m;
    public int MaxHoldingDays { get; set; } = 10;
    public bool UsePartialProfitTaking { get; set; } = true;
}
