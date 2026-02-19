using Microsoft.Extensions.Logging;
using TradeBotEngine.Core.Interfaces;
using TradeBotEngine.Core.Models;

namespace TradeBotEngine.Infrastructure.Services;

/// <summary>
/// Position monitor - checks stop loss, take profit, trailing stops and generates sell signals
/// </summary>
public class PositionMonitorService : IPositionMonitorService
{
    private readonly IPortfolioService _portfolioService;
    private readonly IMarketDataService _marketDataService;
    private readonly IRiskManagementService _riskManagement;
    private readonly IPositionRepository _positionRepo;
    private readonly ILogger<PositionMonitorService> _logger;

    public PositionMonitorService(
        IPortfolioService portfolioService,
        IMarketDataService marketDataService,
        IRiskManagementService riskManagement,
        IPositionRepository positionRepo,
        ILogger<PositionMonitorService> logger)
    {
        _portfolioService = portfolioService;
        _marketDataService = marketDataService;
        _riskManagement = riskManagement;
        _positionRepo = positionRepo;
        _logger = logger;
    }

    public async Task UpdateAllPositionsAsync()
    {
        var positions = await _portfolioService.GetOpenPositionsAsync();
        _logger.LogInformation("Updating {Count} open positions", positions.Count);

        foreach (var position in positions)
        {
            try
            {
                var quote = await _marketDataService.GetQuoteAsync(position.Symbol);
                var price = quote.Price;

                // Update trailing stop
                var newStop = _riskManagement.CalculateTrailingStop(position.EntryPrice, price, position.StopLoss);
                if (newStop > position.StopLoss)
                {
                    position.StopLoss = newStop;
                    position.TrailingStopPrice = newStop;
                    _logger.LogInformation("Trailing stop updated for {Symbol}: ${Stop:F2}", position.Symbol, newStop);
                }

                // Track highest price (peak)
                if (!position.HighestPrice.HasValue || price > position.HighestPrice)
                    position.HighestPrice = price;
                    
                if (price > position.PeakPrice)
                    position.PeakPrice = price;

                // Update P&L
                position.CurrentPrice = price;
                position.UnrealizedPnL = (price - position.EntryPrice) * position.Quantity;
                position.UnrealizedPnLPercent = position.EntryPrice > 0 
                    ? ((price - position.EntryPrice) / position.EntryPrice) * 100 
                    : 0;
                position.DaysHeld = (int)(DateTime.UtcNow - position.CreatedAt).TotalDays;
                position.StopLossDistance = price - position.StopLoss;
                position.TakeProfitDistance = position.TakeProfit - price;
                position.LastUpdated = DateTime.UtcNow;

                // Add daily snapshot
                try
                {
                    var candles = await _marketDataService.GetHistoricalDataAsync(position.Symbol, 2);
                    var latest = candles.LastOrDefault();
                    if (latest != null)
                    {
                        position.DailySnapshots ??= new List<DailySnapshot>();
                        position.DailySnapshots.Add(new DailySnapshot
                        {
                            Date = DateTime.UtcNow.Date,
                            OpenPrice = latest.Open,
                            HighPrice = latest.High,
                            LowPrice = latest.Low,
                            ClosePrice = latest.Close,
                            Price = latest.Close,
                            Volume = latest.Volume,
                            UnrealizedPnL = position.UnrealizedPnL,
                            UnrealizedPnLPercent = position.UnrealizedPnLPercent
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to fetch historical data for {Symbol}", position.Symbol);
                }

                await _positionRepo.UpdateAsync(position);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update position for {Symbol}", position.Symbol);
            }
        }
    }

    public async Task<List<SellSignal>> CheckSellSignalsAsync()
    {
        var positions = await _portfolioService.GetOpenPositionsAsync();
        var signals = new List<SellSignal>();

        foreach (var position in positions)
        {
            var price = position.CurrentPrice;
            var signal = EvaluateSellSignal(position, price);
            if (signal != null)
            {
                signals.Add(signal);
                _logger.LogWarning("SELL SIGNAL [{Symbol}]: {Reason}", position.Symbol, signal.Reason);
            }
        }

        return signals;
    }

    private SellSignal? EvaluateSellSignal(Position position, decimal currentPrice)
    {
        if (position.EntryPrice <= 0) return null;
        
        var gainPct = ((currentPrice - position.EntryPrice) / position.EntryPrice) * 100;
        var peakGainPct = position.PeakPrice > 0 && position.EntryPrice > 0
            ? ((position.PeakPrice - position.EntryPrice) / position.EntryPrice) * 100
            : gainPct;
        
        // Determine weekly trend based on snapshots
        var weeklyTrend = DetermineWeeklyTrend(position);

        // Stop loss hit
        if (currentPrice <= position.StopLoss)
            return new SellSignal
            {
                Symbol = position.Symbol,
                Quantity = position.Quantity,
                AccountId = position.ETradeAccountId,
                CurrentGain = gainPct,
                CurrentGainPercent = gainPct,
                PeakGainPercent = peakGainPct,
                WeeklyTrend = weeklyTrend,
                Reason = $"Stop loss hit (${currentPrice:F2} <= ${position.StopLoss:F2})",
                Confidence = 1.0m,
                AutoExecute = true
            };

        // Take profit hit
        if (currentPrice >= position.TakeProfit)
            return new SellSignal
            {
                Symbol = position.Symbol,
                Quantity = position.Quantity,
                AccountId = position.ETradeAccountId,
                CurrentGain = gainPct,
                CurrentGainPercent = gainPct,
                PeakGainPercent = peakGainPct,
                WeeklyTrend = weeklyTrend,
                Reason = $"Take profit hit (${currentPrice:F2} >= ${position.TakeProfit:F2})",
                Confidence = 1.0m,
                AutoExecute = true
            };

        // Trailing stop hit
        if (position.TrailingStopPrice.HasValue && currentPrice <= position.TrailingStopPrice.Value)
            return new SellSignal
            {
                Symbol = position.Symbol,
                Quantity = position.Quantity,
                AccountId = position.ETradeAccountId,
                CurrentGain = gainPct,
                CurrentGainPercent = gainPct,
                PeakGainPercent = peakGainPct,
                WeeklyTrend = weeklyTrend,
                Reason = $"Trailing stop hit (${currentPrice:F2} <= ${position.TrailingStopPrice.Value:F2})",
                Confidence = 0.95m,
                AutoExecute = true
            };

        // Significant drawdown from peak (pullback from gains)
        if (peakGainPct > 10 && gainPct < peakGainPct * 0.5m)
            return new SellSignal
            {
                Symbol = position.Symbol,
                Quantity = position.Quantity,
                AccountId = position.ETradeAccountId,
                CurrentGain = gainPct,
                CurrentGainPercent = gainPct,
                PeakGainPercent = peakGainPct,
                WeeklyTrend = weeklyTrend,
                Reason = $"Significant drawdown: {peakGainPct:F1}% peak → {gainPct:F1}% current",
                Confidence = 0.75m,
                AutoExecute = false
            };

        // Partial profit signal (don't auto-execute, just alert)
        var (shouldTake, pct) = _riskManagement.ShouldTakePartialProfits(position.EntryPrice, currentPrice);
        if (shouldTake)
            return new SellSignal
            {
                Symbol = position.Symbol,
                Quantity = (int)(position.Quantity * (pct ?? 0.5m)),
                AccountId = position.ETradeAccountId,
                CurrentGain = gainPct,
                CurrentGainPercent = gainPct,
                PeakGainPercent = peakGainPct,
                WeeklyTrend = weeklyTrend,
                Reason = $"Partial profit target reached ({gainPct:F1}% gain)",
                Confidence = 0.80m,
                AutoExecute = false
            };

        return null;
    }

    private TrendDirection DetermineWeeklyTrend(Position position)
    {
        if (position.DailySnapshots == null || position.DailySnapshots.Count < 2)
            return TrendDirection.Neutral;

        var recent = position.DailySnapshots
            .Where(s => s.Date >= DateTime.UtcNow.AddDays(-7))
            .OrderBy(s => s.Date)
            .ToList();

        if (recent.Count < 2)
            return TrendDirection.Neutral;

        var first = recent.First().Price;
        var last = recent.Last().Price;

        if (first <= 0) return TrendDirection.Neutral;
        
        var change = (last - first) / first * 100;

        if (change > 2) return TrendDirection.Up;
        if (change < -2) return TrendDirection.Down;
        return TrendDirection.Neutral;
    }
}
