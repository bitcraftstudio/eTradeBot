using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TradeBotEngine.Core.Interfaces;
using TradeBotEngine.Core.Models;

namespace TradeBotEngine.Infrastructure.Services;

/// <summary>
/// Stock hunter - discovers high-potential stocks via scoring and AI analysis
/// </summary>
public class StockHunterService : IStockHunterService
{
    private readonly IAIAnalysisService _aiService;
    private readonly IMarketDataService _marketDataService;
    private readonly IDiscoveredStockRepository _discoveredStockRepo;
    private readonly IConfiguration _config;
    private readonly ILogger<StockHunterService> _logger;

    private StockHunterConfig _hunterConfig = new()
    {
        Enabled = false,
        MaxDiscoveries = 10,
        Filters = new StockHunterFilters
        {
            MinSmartScore = 8,
            MinUpside = 5,
            MinAnalystRating = 4.0m,
            Limit = 50
        }
    };

    // Default universe to scan - can be extended with TipRanks, Finviz, etc.
    private static readonly string[] DefaultUniverse =
    {
        "AAPL","MSFT","GOOGL","AMZN","NVDA","META","TSLA","AMD","AVGO","ORCL",
        "ADBE","CRWD","PANW","SNPS","KLAC","LRCX","AMAT","TXN","QCOM","MU",
        "JPM","V","MA","BRK.B","GS","BAC","WFC","MS","SPGI","MCO",
        "UNH","LLY","JNJ","ABBV","MRK","PFE","TMO","ABT","DHR","BSX",
        "XOM","CVX","COP","SLB","EOG","PSX","VLO","MPC","OXY","DVN"
    };

    public StockHunterService(
        IAIAnalysisService aiService,
        IMarketDataService marketDataService,
        IDiscoveredStockRepository discoveredStockRepo,
        IConfiguration config,
        ILogger<StockHunterService> logger)
    {
        _aiService = aiService;
        _marketDataService = marketDataService;
        _discoveredStockRepo = discoveredStockRepo;
        _config = config;
        _logger = logger;
    }

    public async Task<StockHunterResult> HuntStocksAsync(StockHunterFilters? filters = null)
    {
        var activeFilters = filters ?? _hunterConfig.Filters;
        var limit = activeFilters.Limit ?? 50;
        var universe = DefaultUniverse.Take(limit).ToArray();

        _logger.LogInformation("=== Stock Hunt started on {Count} symbols ===", universe.Length);

        var result = new StockHunterResult { Timestamp = DateTime.UtcNow };
        var candidates = new List<DiscoveredStock>();

        foreach (var symbol in universe)
        {
            try
            {
                var quote = await _marketDataService.GetQuoteAsync(symbol);
                var candles = await _marketDataService.GetHistoricalDataAsync(symbol, 60);
                var indicators = await _marketDataService.CalculateIndicatorsAsync(symbol, candles);

                // Calculate composite score without paying for AI on every symbol
                var score = ScoreStock(indicators, quote);
                var minScore = activeFilters.MinSmartScore ?? 8;

                if (score >= minScore)
                {
                    // Get AI analysis for top candidates only
                    MarketAnalysis? analysis = null;
                    try { analysis = await _aiService.AnalyzeStockAsync(symbol); }
                    catch (Exception ex) { _logger.LogWarning(ex, "AI analysis skipped for {Symbol}", symbol); }

                    var upside = CalculateUpside(analysis, quote.Price);
                    var minUpside = activeFilters.MinUpside ?? 5m;

                    if (upside >= minUpside)
                    {
                        var discovered = new DiscoveredStock
                        {
                            Symbol = symbol,
                            Name = symbol, // Would be enriched from company profile API
                            SmartScore = score,
                            Rating = analysis?.Recommendation.Decision.ToString() ?? "Hold",
                            CurrentPrice = quote.Price,
                            PriceTarget = analysis?.Recommendation.SuggestedTakeProfit ?? (quote.Price * 1.1m),
                            Upside = upside,
                            TechnicalScore = analysis?.Recommendation.TechnicalScore ?? ScoreTechnicals(indicators),
                            NewsSentiment = analysis?.Sentiment.OverallSentiment ?? 0,
                            DiscoveredAt = DateTime.UtcNow,
                            Reasons = BuildReasons(indicators, analysis, upside)
                        };

                        candidates.Add(discovered);
                        await _discoveredStockRepo.CreateAsync(discovered);
                    }
                }

                result.Filtered++;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error evaluating {Symbol}", symbol);
            }
        }

        result.TotalFound = universe.Length;

        // Rank by composite score
        var ranked = candidates
            .OrderByDescending(c => c.SmartScore * 0.5m + c.Upside * 0.3m + c.TechnicalScore * 0.002m)
            .Take(_hunterConfig.MaxDiscoveries)
            .ToList();

        result.Recommendations = ranked;
        result.Summary = new HunterSummary
        {
            // Fix: Average over int SmartScore yields double; convert each value to decimal so Average returns decimal.
            AvgSmartScore = ranked.Count > 0 ? ranked.Average(r => (decimal)r.SmartScore) : 0m,
            AvgUpside = ranked.Count > 0 ? ranked.Average(r => r.Upside) : 0m,
            TopPick = ranked.FirstOrDefault(),
            SectorsRepresented = new List<string> { "Technology", "Finance", "Healthcare", "Energy" }
        };

        _logger.LogInformation("Hunt complete: {Found} found, {Recs} recommendations, top: {Top}",
            result.TotalFound, ranked.Count, result.Summary.TopPick?.Symbol ?? "none");

        return result;
    }

    public async Task<List<DiscoveredStock>> GetDiscoveredStocksAsync() =>
        await _discoveredStockRepo.GetAllAsync();

    public async Task<bool> AddToWatchlistAsync(string symbol)
    {
        var stock = await _discoveredStockRepo.GetBySymbolAsync(symbol);
        if (stock != null)
        {
            stock.AddedToWatchlist = true;
            await _discoveredStockRepo.UpdateAsync(stock);
            return true;
        }

        // Add new watchlist entry
        var quote = await _marketDataService.GetQuoteAsync(symbol);
        await _discoveredStockRepo.CreateAsync(new DiscoveredStock
        {
            Symbol = symbol,
            Name = symbol,
            CurrentPrice = quote.Price,
            AddedToWatchlist = true,
            DiscoveredAt = DateTime.UtcNow,
            Reasons = new List<string> { "Manually added to watchlist" }
        });
        return true;
    }

    public StockHunterConfig GetConfig() => _hunterConfig;

    public void UpdateConfig(StockHunterConfig config) => _hunterConfig = config;

    // ─── Scoring helpers ──────────────────────────────────────────────────────

    private static int ScoreStock(TechnicalIndicators indicators, StockQuote quote)
    {
        int score = 5; // Base

        // RSI signals
        if (indicators.RSI < 35) score += 2;
        else if (indicators.RSI > 65) score -= 1;

        // MACD bullish
        if (indicators.MACD.Value > indicators.MACD.Signal) score += 1;

        // Price above SMAs
        if (quote.Price > indicators.SMA20) score += 1;
        if (quote.Price > indicators.SMA50) score += 1;

        // Volume above average
        if (indicators.VolumeAvg20 > 0 && indicators.Volume > indicators.VolumeAvg20 * 1.2m) score += 1;

        // EMA alignment
        if (indicators.EMA12 > indicators.EMA26) score += 1;

        return Math.Max(1, Math.Min(10, score));
    }

    private static int ScoreTechnicals(TechnicalIndicators indicators)
    {
        int score = 50;
        if (indicators.RSI < 35) score += 15;
        if (indicators.MACD.Value > indicators.MACD.Signal) score += 15;
        if (indicators.EMA12 > indicators.EMA26) score += 10;
        return Math.Max(0, Math.Min(100, score));
    }

    private static decimal CalculateUpside(MarketAnalysis? analysis, decimal currentPrice)
    {
        if (analysis?.Recommendation.SuggestedTakeProfit.HasValue == true)
        {
            var target = analysis.Recommendation.SuggestedTakeProfit.Value;
            return ((target - currentPrice) / currentPrice) * 100;
        }
        return analysis?.Recommendation.ExpectedReturn * 100 ?? 5m;
    }

    private static List<string> BuildReasons(TechnicalIndicators indicators, MarketAnalysis? analysis, decimal upside)
    {
        var reasons = new List<string>();
        if (indicators.RSI < 35) reasons.Add($"Oversold RSI: {indicators.RSI:F1}");
        if (indicators.MACD.Value > indicators.MACD.Signal) reasons.Add("Bullish MACD crossover");
        if (upside > 10) reasons.Add($"High upside potential: {upside:F1}%");
        if (analysis?.Sentiment.SentimentLabel is "POSITIVE" or "VERY_POSITIVE")
            reasons.Add($"Positive news sentiment: {analysis.Sentiment.SentimentLabel}");
        if (!reasons.Any()) reasons.Add("Technical screening criteria met");
        return reasons;
    }
}
