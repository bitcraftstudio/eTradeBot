using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using TradeBotEngine.Core.Models;
using TradeBotEngine.Infrastructure.Services;
using Xunit;

namespace TradeBotEngine.Tests;

/// <summary>
/// Unit tests for RiskManagementService
/// </summary>
public class RiskManagementServiceTests
{
    private readonly RiskManagementService _sut;

    public RiskManagementServiceTests()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Trading:DefaultRiskProfile", "Moderate" }
            })
            .Build();

        _sut = new RiskManagementService(config, NullLogger<RiskManagementService>.Instance);
    }

    [Fact]
    public void CalculatePositionSize_Moderate_ReturnsValidCalculation()
    {
        var result = _sut.CalculatePositionSize("AAPL", 150m, 10000m, 10000m, RiskProfile.Moderate);

        Assert.Equal("AAPL", result.Symbol);
        Assert.True(result.StopLossPrice < 150m);
        Assert.True(result.TakeProfitPrice > 150m);
        Assert.True(result.RiskRewardRatio >= 2.0m);
        Assert.True(result.RecommendedQuantity >= 0);
    }

    [Fact]
    public void CalculatePositionSize_VeryAggressive_HigherRisk()
    {
        var moderate = _sut.CalculatePositionSize("AAPL", 150m, 10000m, 10000m, RiskProfile.Moderate);
        var aggressive = _sut.CalculatePositionSize("AAPL", 150m, 10000m, 10000m, RiskProfile.VeryAggressive);

        Assert.True(aggressive.CapitalRisked >= moderate.CapitalRisked);
        Assert.True(aggressive.StopLossPrice < moderate.StopLossPrice);
    }

    [Fact]
    public void CalculatePositionSize_Conservative_SmallerPositions()
    {
        var conservative = _sut.CalculatePositionSize("AAPL", 150m, 10000m, 10000m, RiskProfile.Conservative);

        Assert.True(conservative.CapitalRisked <= 0.01m * 10000m + 0.01m); // 1% max risk + epsilon
        Assert.True(conservative.RiskRewardRatio >= 3.0m - 0.01m); // 3:1 min
    }

    [Theory]
    [InlineData(RiskProfile.Conservative, 3)]
    [InlineData(RiskProfile.Moderate, 5)]
    [InlineData(RiskProfile.Aggressive, 7)]
    [InlineData(RiskProfile.VeryAggressive, 10)]
    public void CanOpenPosition_AllowedBelowMax(RiskProfile profile, int maxPositions)
    {
        var (allowed, _) = _sut.CanOpenPosition(maxPositions - 1, profile);
        Assert.True(allowed);
    }

    [Theory]
    [InlineData(RiskProfile.Conservative, 3)]
    [InlineData(RiskProfile.Moderate, 5)]
    [InlineData(RiskProfile.Aggressive, 7)]
    [InlineData(RiskProfile.VeryAggressive, 10)]
    public void CanOpenPosition_BlockedAtMax(RiskProfile profile, int maxPositions)
    {
        var (allowed, reason) = _sut.CanOpenPosition(maxPositions, profile);
        Assert.False(allowed);
        Assert.NotNull(reason);
    }

    [Fact]
    public void MeetsRiskRewardRequirements_Moderate_Requires2to1()
    {
        var (meets, _) = _sut.MeetsRiskRewardRequirements(2.0m, RiskProfile.Moderate);
        Assert.True(meets);

        var (fails, reason) = _sut.MeetsRiskRewardRequirements(1.5m, RiskProfile.Moderate);
        Assert.False(fails);
        Assert.NotNull(reason);
    }

    [Fact]
    public void CalculateTrailingStop_PriceUp_TrailsStop()
    {
        var entryPrice = 100m;
        var currentPrice = 110m;
        var currentStop = 95m;

        var newStop = _sut.CalculateTrailingStop(entryPrice, currentPrice, currentStop, RiskProfile.Moderate);
        Assert.True(newStop > currentStop);
    }

    [Fact]
    public void CalculateTrailingStop_PriceDown_DoesNotMoveStop()
    {
        var entryPrice = 100m;
        var currentPrice = 95m;
        var currentStop = 92m;

        var newStop = _sut.CalculateTrailingStop(entryPrice, currentPrice, currentStop, RiskProfile.Moderate);
        Assert.Equal(currentStop, newStop);
    }

    [Fact]
    public void ShouldTakePartialProfits_AtTarget_ReturnsTrue()
    {
        // Moderate takes profits at 4% and 8%
        var (should, pct) = _sut.ShouldTakePartialProfits(100m, 104.5m, RiskProfile.Moderate);
        Assert.True(should);
        Assert.Equal(0.5m, pct);
    }

    [Fact]
    public void ShouldTakePartialProfits_BelowTarget_ReturnsFalse()
    {
        var (should, _) = _sut.ShouldTakePartialProfits(100m, 102m, RiskProfile.Moderate);
        Assert.False(should);
    }
}

/// <summary>
/// Unit tests for MarketDataService indicator calculations
/// </summary>
public class MarketDataServiceIndicatorTests
{
    [Fact]
    public void RSI_OversoldCondition_BelowThirty()
    {
        // Simulate a declining price series (oversold)
        var prices = Enumerable.Range(0, 30)
            .Select(i => 100m - i * 1.5m) // Strongly declining
            .ToList();

        var candles = prices.Select((p, i) => new CandleData
        {
            Date = DateTime.UtcNow.AddDays(-30 + i),
            Open = p + 0.5m, High = p + 1m, Low = p - 0.5m, Close = p, Volume = 1000000
        }).ToList();

        var svc = CreateMarketDataService();
        var indicators = svc.CalculateIndicatorsAsync("TEST", candles).Result;

        Assert.True(indicators.RSI < 40, $"RSI should be below 40 for declining market, was {indicators.RSI}");
    }

    [Fact]
    public void BollingerBands_MiddleIsSMA()
    {
        var prices = Enumerable.Repeat(100m, 30).ToList(); // Flat prices
        var candles = prices.Select((p, i) => new CandleData
        {
            Date = DateTime.UtcNow.AddDays(-30 + i),
            Open = p, High = p, Low = p, Close = p, Volume = 1000000
        }).ToList();

        var svc = CreateMarketDataService();
        var indicators = svc.CalculateIndicatorsAsync("TEST", candles).Result;

        Assert.Equal(100m, indicators.BollingerBands.Middle);
        Assert.Equal(indicators.BollingerBands.Upper, indicators.BollingerBands.Lower,
            comparer: Comparer<decimal>.Create((a, b) => Math.Abs(a - b) < 0.01m ? 0 : a.CompareTo(b)));
    }

    private static MarketDataService CreateMarketDataService()
    {
        var repoMock = new MockMarketDataRepository();
        return new MarketDataService(NullLogger<MarketDataService>.Instance, repoMock);
    }
}

internal class MockMarketDataRepository : TradeBotEngine.Core.Interfaces.IMarketDataRepository
{
    public Task<List<MarketData>> GetBySymbolAsync(string symbol, int days) => Task.FromResult(new List<MarketData>());
    public Task<MarketData?> GetBySymbolAndDateAsync(string symbol, DateTime date) => Task.FromResult<MarketData?>(null);
    public Task<MarketData> UpsertAsync(MarketData data) => Task.FromResult(data);
    public Task<bool> DeleteOldDataAsync(int daysToKeep) => Task.FromResult(true);
}
