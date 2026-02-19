namespace TradeBotEngine.Core.Models;

/// <summary>
/// Represents an active position in the portfolio
/// </summary>
public class Position
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string TradeId { get; set; } = string.Empty;
    public string? TradeObjectId { get; set; }
    public string Symbol { get; set; } = string.Empty;
    
    // Position details
    public int Quantity { get; set; }
    public decimal EntryPrice { get; set; }
    public decimal CurrentPrice { get; set; }
    public decimal UnrealizedPnL { get; set; }
    public decimal UnrealizedPnLPercent { get; set; }
    
    // Holding period
    public int DaysHeld { get; set; }
    public DateTime EntryDate { get; set; } = DateTime.UtcNow;
    
    // Risk levels
    public decimal StopLoss { get; set; }
    public decimal TakeProfit { get; set; }
    public decimal StopLossDistance { get; set; }
    public decimal TakeProfitDistance { get; set; }
    
    // Trailing stop & peak tracking
    public decimal? HighestPrice { get; set; }
    public decimal PeakPrice { get; set; }
    public decimal? TrailingStopPrice { get; set; }
    
    // Daily snapshots for analysis
    public List<DailySnapshot> DailySnapshots { get; set; } = new();
    
    // Status
    public PositionStatus Status { get; set; } = PositionStatus.Open;
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    
    // eTrade specific
    public string? ETradePositionId { get; set; }
    public string? ETradeAccountId { get; set; }
}

public enum PositionStatus
{
    Open,
    Closed,
    PendingSell
}

public class DailySnapshot
{
    public DateTime Date { get; set; }
    public decimal Price { get; set; }  // Alias for ClosePrice for easy access
    public decimal OpenPrice { get; set; }
    public decimal HighPrice { get; set; }
    public decimal LowPrice { get; set; }
    public decimal ClosePrice { get; set; }
    public long Volume { get; set; }
    public decimal UnrealizedPnL { get; set; }
    public decimal UnrealizedPnLPercent { get; set; }
    
    // Technical snapshot
    public decimal? RSI { get; set; }
    public decimal? MACDValue { get; set; }
    public decimal? MACDSignal { get; set; }
}
