using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using TradeBotEngine.Core.Interfaces;
using TradeBotEngine.Core.Models;

namespace TradeBotEngine.Functions.Timers;

public class Scheduler
{
    private readonly ILogger _logger;
    private readonly ITradingService _tradingService;
    private readonly IPortfolioService _portfolioService;
    private readonly IPositionMonitorService _positionMonitor;
    private readonly IMarketDataService _marketDataService;
    private readonly IAIAnalysisService _aiAnalysisService;
    private readonly ILearningService _learningService;
    private readonly IStockHunterService _stockHunter;
    private readonly IMarketScanService _marketScanService;

    public Scheduler(
        ITradingService tradingService,
        IPortfolioService portfolioService,
        IPositionMonitorService positionMonitor,
        IMarketDataService marketDataService,
        IAIAnalysisService aiAnalysisService,
        ILearningService learningService,
        IStockHunterService stockHunter,
        IMarketScanService marketScanService,
        ILoggerFactory loggerFactory)
    {
        _tradingService = tradingService;
        _portfolioService = portfolioService;
        _positionMonitor = positionMonitor;
        _marketDataService = marketDataService;
        _aiAnalysisService = aiAnalysisService;
        _learningService = learningService;
        _stockHunter = stockHunter;
        _marketScanService = marketScanService;
        _logger = loggerFactory.CreateLogger<Scheduler>();
    }

    /// <summary>
    /// Morning market scan - 9:30 AM ET Mon-Fri
    /// Scans watchlist, gets AI analysis, auto-trades high-confidence opportunities
    /// CRON: "0 30 14 * * 1-5" (UTC = 14:30 = 9:30 AM ET)
    /// </summary>
    [Function("MorningScan")]
    public async Task MorningScan([TimerTrigger("0 30 14 * * 1-5")] TimerInfo myTimer)
    {
        _logger.LogInformation("=== Morning Market Scan Started ===");

        try
        {
            var result = await _marketScanService.PerformMarketScanAsync();
            _logger.LogInformation(
                "Morning Scan Complete: {Analyzed} analyzed, {Executed} trades executed",
                result.SymbolsAnalyzed, result.TradesExecuted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Morning Market Scan failed");
        }
    }

    /// <summary>
    /// Position monitoring - every 30 min during market hours 10 AM - 4 PM ET
    /// Checks stop loss, take profit, trailing stops, and sell signals
    /// CRON: "0 */30 15-21 * * 1-5" (UTC 15:00-21:00 = 10:00-16:00 ET)
    /// </summary>
    [Function("PositionMonitor")]
    public async Task PositionMonitor([TimerTrigger("0 */30 15-21 * * 1-5")] TimerInfo timer)
    {
        _logger.LogInformation("=== Position Monitor Running ===");

        try
        {
            await _positionMonitor.UpdateAllPositionsAsync();
            var sellSignals = await _positionMonitor.CheckSellSignalsAsync();

            foreach (var signal in sellSignals)
            {
                _logger.LogWarning(
                    "SELL SIGNAL [{Symbol}]: {Reason} | Gain: {Gain:F2}% | Confidence: {Conf:F0}%",
                    signal.Symbol, signal.Reason, signal.CurrentGain, signal.Confidence * 100);

                // Auto-execute sell if enabled (controlled by Trading:AutoTradeEnabled config)
                if (signal.AutoExecute)
                {
                    try
                    {
                        await _tradingService.ExecuteTradeAsync(new ExecuteTradeRequest
                        {
                            Symbol = signal.Symbol,
                            Type = TradeType.Sell,
                            Quantity = signal.Quantity,
                            AccountId = signal.AccountId
                        });
                        _logger.LogInformation("Auto-sold {Symbol} @ gain {Gain:F2}%", signal.Symbol, signal.CurrentGain);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Auto-sell failed for {Symbol}", signal.Symbol);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Position monitoring failed");
        }
    }

    /// <summary>
    /// Daily position snapshot - 4:30 PM ET Mon-Fri (after market close)
    /// Records EOD prices and updates daily performance tracking
    /// CRON: "0 30 21 * * 1-5" (UTC 21:30 = 4:30 PM ET)
    /// </summary>
    [Function("DailyPositionSnapshot")]
    public async Task DailyPositionSnapshot([TimerTrigger("0 30 21 * * 1-5")] TimerInfo timer)
    {
        _logger.LogInformation("=== Daily Position Snapshot ===");

        try
        {
            await _positionMonitor.UpdateAllPositionsAsync();
            _logger.LogInformation("End-of-day snapshots saved for all open positions");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Daily snapshot failed");
        }
    }

    /// <summary>
    /// Daily learning review - 5:30 PM ET Mon-Fri
    /// AI reviews closed trades, extracts patterns, and refines strategy weights
    /// CRON: "0 30 22 * * 1-5" (UTC 22:30 = 5:30 PM ET)
    /// </summary>
    [Function("DailyLearningReview")]
    public async Task DailyLearningReview([TimerTrigger("0 30 22 * * 1-5")] TimerInfo timer)
    {
        _logger.LogInformation("=== Daily Learning Review ===");

        try
        {
            var review = await _learningService.PerformDailyReviewAsync(7);
            _logger.LogInformation(
                "Learning review complete: {Trades} trades, {WinRate:F1}% win rate, PnL: ${PnL:F2}",
                review.TradesReviewed, review.WinRate * 100, review.TotalProfitLoss);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Daily learning review failed");
        }
    }

    /// <summary>
    /// Weekly stock hunt - Every Monday 8:00 AM ET
    /// Discovers new high-potential stocks and adds to watchlist
    /// CRON: "0 0 13 * * 1" (UTC 13:00 Mon = 8:00 AM ET Mon)
    /// </summary>
    [Function("WeeklyStockHunt")]
    public async Task WeeklyStockHunt([TimerTrigger("0 0 13 * * 1")] TimerInfo timer)
    {
        _logger.LogInformation("=== Weekly Stock Hunt ===");

        try
        {
            var result = await _stockHunter.HuntStocksAsync();
            _logger.LogInformation(
                "Stock hunt complete: {Found} found, {Filtered} after filters, top pick: {TopPick}",
                result.TotalFound, result.Filtered, result.Summary.TopPick?.Symbol ?? "none");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Weekly stock hunt failed");
        }
    }
}