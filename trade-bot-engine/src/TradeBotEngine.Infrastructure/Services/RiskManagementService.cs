using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TradeBotEngine.Core.Interfaces;
using TradeBotEngine.Core.Models;

namespace TradeBotEngine.Infrastructure.Services;

/// <summary>
/// Risk management service for position sizing and risk control
/// </summary>
public class RiskManagementService : IRiskManagementService
{
    private readonly ILogger<RiskManagementService> _logger;
    private readonly RiskProfile _defaultRiskProfile;
    private const decimal Epsilon = 0.01m;

    public RiskManagementService(
        IConfiguration configuration,
        ILogger<RiskManagementService> logger)
    {
        _logger = logger;
        
        var profileStr = configuration["Trading:DefaultRiskProfile"] ?? "Moderate";
        _defaultRiskProfile = Enum.TryParse<RiskProfile>(profileStr, true, out var profile) 
            ? profile 
            : RiskProfile.Moderate;

        _logger.LogInformation("Risk management initialized with default profile: {Profile}", _defaultRiskProfile);
    }

    public RiskCalculation CalculatePositionSize(
        string symbol,
        decimal currentPrice,
        decimal portfolioValue,
        decimal cashAvailable,
        RiskProfile? riskProfile = null)
    {
        var profile = riskProfile ?? _defaultRiskProfile;
        var config = RiskProfiles.GetConfig(profile);

        _logger.LogDebug("Calculating position size for {Symbol} with {Profile} profile", symbol, profile);

        // Calculate stop loss price (X% below entry)
        var stopLossPrice = currentPrice * (1 - config.StopLossPercent);

        // Calculate take profit price (based on risk-reward ratio)
        var takeProfitPrice = currentPrice * (1 + (config.StopLossPercent * config.MinRiskReward));

        // Maximum position value (% of portfolio)
        var maxPositionValue = portfolioValue * config.MaxPositionPercent;

        // Maximum capital to risk on this trade
        var maxCapitalRisk = portfolioValue * config.MaxRiskPerTrade;

        // Calculate position size based on capital risk
        var riskPerShare = currentPrice - stopLossPrice;
        var quantityBasedOnRisk = riskPerShare > 0 
            ? (int)Math.Floor(maxCapitalRisk / riskPerShare) 
            : 0;

        // Calculate position size based on max position percent
        var quantityBasedOnPosition = currentPrice > 0 
            ? (int)Math.Floor(maxPositionValue / currentPrice) 
            : 0;

        // Use the smaller of the two (more conservative)
        var recommendedQuantity = Math.Min(quantityBasedOnRisk, quantityBasedOnPosition);

        // Make sure we have enough cash
        var maxQuantityFromCash = currentPrice > 0 
            ? (int)Math.Floor(cashAvailable / currentPrice) 
            : 0;
        recommendedQuantity = Math.Min(recommendedQuantity, maxQuantityFromCash);

        // Ensure at least 1 share (if possible)
        if (recommendedQuantity < 1 && cashAvailable >= currentPrice)
        {
            recommendedQuantity = 1;
        }

        var positionValue = recommendedQuantity * currentPrice;
        var capitalRisked = recommendedQuantity * (currentPrice - stopLossPrice);
        var riskRewardRatio = riskPerShare > 0 
            ? (takeProfitPrice - currentPrice) / riskPerShare 
            : 0;

        _logger.LogInformation(
            "Position calculation for {Symbol}: Qty={Quantity}, Value=${Value:F2}, Risk=${Risk:F2}, R:R={Ratio:F2}",
            symbol, recommendedQuantity, positionValue, capitalRisked, riskRewardRatio);

        return new RiskCalculation
        {
            Symbol = symbol,
            CurrentPrice = currentPrice,
            RiskProfile = profile.ToString(),
            PortfolioValue = portfolioValue,
            CashAvailable = cashAvailable,
            RecommendedQuantity = recommendedQuantity,
            MaxQuantity = maxQuantityFromCash,
            PositionValue = positionValue,
            CapitalRisked = capitalRisked,
            StopLossPrice = stopLossPrice,
            TakeProfitPrice = takeProfitPrice,
            RiskRewardRatio = riskRewardRatio
        };
    }

    public (bool Allowed, string? Reason) CanOpenPosition(int currentOpenPositions, RiskProfile? riskProfile = null)
    {
        var profile = riskProfile ?? _defaultRiskProfile;
        var config = RiskProfiles.GetConfig(profile);

        if (currentOpenPositions >= config.MaxOpenPositions)
        {
            return (false, $"Maximum open positions ({config.MaxOpenPositions}) reached for {profile} risk profile");
        }

        return (true, null);
    }

    public (bool Meets, string? Reason) MeetsRiskRewardRequirements(decimal riskRewardRatio, RiskProfile? riskProfile = null)
    {
        var profile = riskProfile ?? _defaultRiskProfile;
        var config = RiskProfiles.GetConfig(profile);

        // Use epsilon tolerance for floating-point comparison
        if (riskRewardRatio < (config.MinRiskReward - Epsilon))
        {
            return (false, $"Risk/reward ratio {riskRewardRatio:F2}:1 is below minimum {config.MinRiskReward}:1");
        }

        return (true, null);
    }

    public decimal CalculateTrailingStop(
        decimal entryPrice,
        decimal currentPrice,
        decimal currentStopLoss,
        RiskProfile? riskProfile = null)
    {
        var profile = riskProfile ?? _defaultRiskProfile;
        var config = RiskProfiles.GetConfig(profile);

        // If price moved up, trail the stop
        if (currentPrice > entryPrice)
        {
            var newStopLoss = currentPrice * (1 - config.TrailingStopPercent);

            // Only update if new stop is higher than current stop
            if (newStopLoss > currentStopLoss)
            {
                _logger.LogDebug("Trailing stop updated: ${Old:F2} → ${New:F2}", currentStopLoss, newStopLoss);
                return newStopLoss;
            }
        }

        return currentStopLoss;
    }

    public (bool ShouldTake, decimal? Percentage) ShouldTakePartialProfits(
        decimal entryPrice,
        decimal currentPrice,
        RiskProfile? riskProfile = null)
    {
        var profile = riskProfile ?? _defaultRiskProfile;
        var config = RiskProfiles.GetConfig(profile);

        var gainPercent = (currentPrice - entryPrice) / entryPrice;

        foreach (var targetPercent in config.PartialProfitLevels)
        {
            if (gainPercent >= targetPercent)
            {
                // Take 50% at each level
                return (true, 0.5m);
            }
        }

        return (false, null);
    }
}
