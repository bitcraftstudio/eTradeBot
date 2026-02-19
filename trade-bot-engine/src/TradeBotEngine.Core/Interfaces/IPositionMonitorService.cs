using TradeBotEngine.Core.Models;

namespace TradeBotEngine.Core.Interfaces;

/// <summary>
/// Position monitoring service - checks stop loss, take profit, trailing stops
/// </summary>
public interface IPositionMonitorService
{
    Task UpdateAllPositionsAsync();
    Task<List<SellSignal>> CheckSellSignalsAsync();
}

/// <summary>
/// Signal generated when a position should be sold
/// </summary>
public class SellSignal
{
    public string Symbol { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string? AccountId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public decimal Confidence { get; set; }
    public bool AutoExecute { get; set; }
    
    // Gain metrics
    public decimal CurrentGain { get; set; }
    public decimal CurrentGainPercent { get; set; }
    public decimal PeakGainPercent { get; set; }
    
    // Trend
    public TrendDirection WeeklyTrend { get; set; } = TrendDirection.Neutral;
}

public enum TrendDirection
{
    Up,
    Down,
    Neutral
}
