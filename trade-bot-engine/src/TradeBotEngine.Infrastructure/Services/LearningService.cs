using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TradeBotEngine.Core.Interfaces;
using TradeBotEngine.Core.Models;

namespace TradeBotEngine.Infrastructure.Services;

/// <summary>
/// Learning service - AI-powered review of closed trades, pattern extraction, and strategy adaptation
/// </summary>
public class LearningService : ILearningService
{
    private readonly IAIAnalysisService _aiService;
    private readonly ITradeRepository _tradeRepo;
    private readonly ILearningInsightRepository _insightRepo;
    private readonly ILogger<LearningService> _logger;

    public LearningService(
        IAIAnalysisService aiService,
        ITradeRepository tradeRepo,
        ILearningInsightRepository insightRepo,
        ILogger<LearningService> logger)
    {
        _aiService = aiService;
        _tradeRepo = tradeRepo;
        _insightRepo = insightRepo;
        _logger = logger;
    }

    public async Task<LearningReview> PerformDailyReviewAsync(int days = 7)
    {
        _logger.LogInformation("Starting learning review for last {Days} days", days);

        var since = DateTime.UtcNow.AddDays(-days);
        var trades = await _tradeRepo.GetByDateRangeAsync(since, DateTime.UtcNow);
        var closedTrades = trades.Where(t => t.Status == TradeStatus.Closed).ToList();

        var review = new LearningReview
        {
            Date = DateTime.UtcNow,
            TradesReviewed = closedTrades.Count
        };

        if (closedTrades.Count == 0)
        {
            _logger.LogInformation("No closed trades in last {Days} days", days);
            return review;
        }

        // ── Compute performance metrics ───────────────────────────────────────
        var profitable = closedTrades.Where(t => t.Outcome?.ProfitLoss > 0).ToList();
        review.SuccessfulTrades = profitable.Count;
        review.FailedTrades = closedTrades.Count - profitable.Count;
        review.WinRate = closedTrades.Count > 0 ? (decimal)review.SuccessfulTrades / closedTrades.Count : 0;
        review.TotalProfitLoss = closedTrades.Sum(t => t.Outcome?.ProfitLoss ?? 0);
        review.AvgReturn = closedTrades.Average(t => t.Outcome?.ProfitLossPercent ?? 0);

        _logger.LogInformation("Performance: {WinRate:P0} win rate, ${PnL:F2} total P/L", review.WinRate, review.TotalProfitLoss);

        // ── AI pattern extraction ─────────────────────────────────────────────
        try
        {
            var analysis = await _aiService.ExtractLearningsAsync(closedTrades);
            review.Insights = new LearningInsights
            {
                SuccessFactors = analysis.SuccessfulPatterns,
                FailureFactors = analysis.FailedPatterns,
                Recommendations = analysis.Recommendations
            };

            // ── Extract patterns ──────────────────────────────────────────────
            review.Patterns = ExtractPatterns(closedTrades);

            // ── Persist insight ───────────────────────────────────────────────
            var insight = new LearningInsight
            {
                Date = DateTime.UtcNow,
                TradesReviewed = review.TradesReviewed,
                SuccessfulTrades = review.SuccessfulTrades,
                FailedTrades = review.FailedTrades,
                TotalProfitLoss = review.TotalProfitLoss,
                AvgReturn = review.AvgReturn,
                WinRate = review.WinRate,
                Patterns = review.Patterns,
                SuccessFactors = analysis.SuccessfulPatterns,
                FailureFactors = analysis.FailedPatterns,
                Adjustments = analysis.Recommendations,
                AIReflection = analysis.KeyInsights,
                AIModel = "claude-opus-4-5"
            };
            await _insightRepo.CreateAsync(insight);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI learning extraction failed");
        }

        return review;
    }

    public async Task<List<LearningInsight>> GetAllInsightsAsync() =>
        await _insightRepo.GetAllAsync();

    public async Task<LearningInsight?> GetLatestInsightAsync() =>
        await _insightRepo.GetLatestAsync();

    public async Task<PerformanceMetrics> GetPerformanceMetricsAsync()
    {
        var allClosed = await _tradeRepo.GetByStatusAsync(TradeStatus.Closed);

        if (allClosed.Count == 0)
            return new PerformanceMetrics();

        var profitable = allClosed.Where(t => t.Outcome?.ProfitLoss > 0).ToList();
        var best = allClosed.OrderByDescending(t => t.Outcome?.ProfitLossPercent ?? 0).First();
        var worst = allClosed.OrderBy(t => t.Outcome?.ProfitLossPercent ?? 0).First();

        return new PerformanceMetrics
        {
            TotalTrades = allClosed.Count,
            WinRate = allClosed.Count > 0 ? (decimal)profitable.Count / allClosed.Count : 0,
            AvgReturn = allClosed.Average(t => t.Outcome?.ProfitLossPercent ?? 0),
            TotalProfitLoss = allClosed.Sum(t => t.Outcome?.ProfitLoss ?? 0),
            AvgHoldingDays = allClosed.Average(t => (decimal)(t.Outcome?.HoldingDays ?? 0)),
            BestTrade = new TradeInfo { TradeId = best.TradeId, Symbol = best.Symbol, Return = best.Outcome?.ProfitLossPercent ?? 0 },
            WorstTrade = new TradeInfo { TradeId = worst.TradeId, Symbol = worst.Symbol, Return = worst.Outcome?.ProfitLossPercent ?? 0 }
        };
    }

    public async Task<StrategyWeights> GetCurrentStrategyWeightsAsync()
    {
        var latest = await _insightRepo.GetLatestAsync();
        if (latest == null) return new StrategyWeights { RiskProfile = "Moderate" };

        // Derive weights from recent performance patterns
        var allClosed = await _tradeRepo.GetByStatusAsync(TradeStatus.Closed);
        var weights = new StrategyWeights
        {
            RiskProfile = "Moderate",
            TotalTradesAnalyzed = allClosed.Count,
            OverallWinRate = allClosed.Count > 0
                ? (decimal)allClosed.Count(t => t.Outcome?.ProfitLoss > 0) / allClosed.Count
                : 0
        };

        return weights;
    }

    public async Task UpdateStrategyWeightsAsync(StrategyWeights weights)
    {
        // Strategy weights are derived from insights in this implementation.
        // For custom overrides, persist as a separate document.
        _logger.LogInformation("Strategy weights updated for profile {Profile}", weights.RiskProfile);
        await Task.CompletedTask;
    }

    // ─── Pattern extraction ───────────────────────────────────────────────────

    private List<PatternInsight> ExtractPatterns(List<Trade> trades)
    {
        var patterns = new List<PatternInsight>();

        // RSI oversold buy pattern
        var rsiOversoldTrades = trades.Where(t => t.ContextAtEntry?.TechnicalIndicators?.RSI < 35).ToList();
        if (rsiOversoldTrades.Count >= 2)
        {
            var successRate = rsiOversoldTrades.Count(t => t.Outcome?.ProfitLoss > 0) / (decimal)rsiOversoldTrades.Count;
            patterns.Add(new PatternInsight
            {
                Pattern = "RSI_OVERSOLD_BUY",
                Description = "Buy when RSI < 35",
                Occurrences = rsiOversoldTrades.Count,
                SuccessRate = successRate,
                AvgReturn = rsiOversoldTrades.Average(t => t.Outcome?.ProfitLossPercent ?? 0),
                Confidence = successRate > 0.6m ? PatternConfidence.High : PatternConfidence.Medium
            });
        }

        // MACD bullish crossover
        var macdBullish = trades.Where(t => t.ContextAtEntry?.TechnicalIndicators?.MACD.Value > t.ContextAtEntry?.TechnicalIndicators?.MACD.Signal).ToList();
        if (macdBullish.Count >= 2)
        {
            var successRate = macdBullish.Count(t => t.Outcome?.ProfitLoss > 0) / (decimal)macdBullish.Count;
            patterns.Add(new PatternInsight
            {
                Pattern = "MACD_BULLISH_CROSSOVER",
                Description = "MACD line above signal at entry",
                Occurrences = macdBullish.Count,
                SuccessRate = successRate,
                AvgReturn = macdBullish.Average(t => t.Outcome?.ProfitLossPercent ?? 0),
                Confidence = PatternConfidence.Medium
            });
        }

        return patterns;
    }
}
