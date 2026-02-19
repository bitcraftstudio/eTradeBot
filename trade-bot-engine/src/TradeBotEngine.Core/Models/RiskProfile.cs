namespace TradeBotEngine.Core.Models;

/// <summary>
/// Risk profile configuration for trading strategy
/// </summary>
public class RiskProfileConfig
{
    public decimal MaxRiskPerTrade { get; set; } // % of portfolio
    public decimal MaxPositionPercent { get; set; } // % of portfolio per position
    public int MaxOpenPositions { get; set; }
    public int PreferredHoldDaysMin { get; set; }
    public int PreferredHoldDaysMax { get; set; }
    public decimal StopLossPercent { get; set; }
    public decimal MinRiskReward { get; set; }
    public List<string> StockTypes { get; set; } = new();
    public decimal MaxVix { get; set; }
    public decimal TrailingStopPercent { get; set; }
    public List<decimal> PartialProfitLevels { get; set; } = new();
}

public enum RiskProfile
{
    Conservative,
    Moderate,
    Aggressive,
    VeryAggressive
}

public static class RiskProfiles
{
    public static readonly Dictionary<RiskProfile, RiskProfileConfig> Configurations = new()
    {
        [RiskProfile.Conservative] = new RiskProfileConfig
        {
            MaxRiskPerTrade = 0.01m, // 1% max loss per trade
            MaxPositionPercent = 0.10m, // 10% per position
            MaxOpenPositions = 3,
            PreferredHoldDaysMin = 5,
            PreferredHoldDaysMax = 10,
            StopLossPercent = 0.02m, // 2% stop loss
            MinRiskReward = 3.0m, // 3:1 minimum
            StockTypes = new List<string> { "large-cap", "blue-chip" },
            MaxVix = 20m,
            TrailingStopPercent = 0.02m,
            PartialProfitLevels = new List<decimal> { 0.03m, 0.06m } // Take profits at 3%, 6%
        },

        [RiskProfile.Moderate] = new RiskProfileConfig
        {
            MaxRiskPerTrade = 0.02m, // 2% max loss per trade
            MaxPositionPercent = 0.15m, // 15% per position
            MaxOpenPositions = 5,
            PreferredHoldDaysMin = 2,
            PreferredHoldDaysMax = 7,
            StopLossPercent = 0.03m, // 3% stop loss
            MinRiskReward = 2.0m, // 2:1 minimum
            StockTypes = new List<string> { "large-cap", "mid-cap" },
            MaxVix = 25m,
            TrailingStopPercent = 0.03m,
            PartialProfitLevels = new List<decimal> { 0.04m, 0.08m } // Take profits at 4%, 8%
        },

        [RiskProfile.Aggressive] = new RiskProfileConfig
        {
            MaxRiskPerTrade = 0.05m, // 5% max loss per trade
            MaxPositionPercent = 0.25m, // 25% per position
            MaxOpenPositions = 7,
            PreferredHoldDaysMin = 1,
            PreferredHoldDaysMax = 5,
            StopLossPercent = 0.05m, // 5% stop loss
            MinRiskReward = 1.5m, // 1.5:1 minimum
            StockTypes = new List<string> { "mid-cap", "growth" },
            MaxVix = 30m,
            TrailingStopPercent = 0.04m,
            PartialProfitLevels = new List<decimal> { 0.06m, 0.12m } // Take profits at 6%, 12%
        },

        [RiskProfile.VeryAggressive] = new RiskProfileConfig
        {
            MaxRiskPerTrade = 0.10m, // 10% max loss per trade
            MaxPositionPercent = 0.40m, // 40% per position
            MaxOpenPositions = 10,
            PreferredHoldDaysMin = 1,
            PreferredHoldDaysMax = 3,
            StopLossPercent = 0.08m, // 8% stop loss
            MinRiskReward = 1.0m, // 1:1 minimum
            StockTypes = new List<string> { "small-cap", "growth", "volatile" },
            MaxVix = 40m,
            TrailingStopPercent = 0.06m,
            PartialProfitLevels = new List<decimal> { 0.10m, 0.20m } // Take profits at 10%, 20%
        }
    };

    public static RiskProfileConfig GetConfig(RiskProfile profile)
    {
        return Configurations[profile];
    }
}
