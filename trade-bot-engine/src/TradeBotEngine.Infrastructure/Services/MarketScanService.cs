using Microsoft.Extensions.Logging;
using TradeBotEngine.Core.Interfaces;
using TradeBotEngine.Core.Models;

namespace TradeBotEngine.Infrastructure.Services;

public class MarketScanService : IMarketScanService
{
    private readonly IStockHunterService _stockHunter;
    private readonly IAIAnalysisService _aiAnalysisService;
    private readonly ILogger<MarketScanService> _logger;

    public MarketScanService(
        IStockHunterService stockHunter,
        IAIAnalysisService aiAnalysisService,
        ILogger<MarketScanService> logger)
    {
        _stockHunter = stockHunter;
        _aiAnalysisService = aiAnalysisService;
        _logger = logger;
    }

    public async Task<MarketScanResult> PerformMarketScanAsync()
    {
        var config = _stockHunter.GetConfig();
        var watchlist = (config.Filters.ExcludeSymbols?.Any() == true)
            ? new[] { "AAPL", "MSFT", "GOOGL", "TSLA", "NVDA", "AMZN", "META" }
            : new[] { "AAPL", "MSFT", "GOOGL", "TSLA", "NVDA", "AMZN", "META" };

        var result = new MarketScanResult
        {
            Timestamp = DateTime.UtcNow,
            Symbols = watchlist.ToList()
        };

        foreach (var symbol in watchlist)
        {
            try
            {
                _logger.LogInformation("Scanning {Symbol}...", symbol);

                var analysis = await _aiAnalysisService.AnalyzeStockAsync(symbol);
                var rec = analysis.Recommendation;

                result.Recommendations.Add(new ScanRecommendation
                {
                    Symbol = symbol,
                    Decision = rec.Decision.ToString(),
                    Confidence = rec.Confidence,
                    Reasoning = rec.Reasoning,
                    Price = analysis.CurrentPrice
                });

                result.SymbolsAnalyzed++;

                _logger.LogInformation("{Symbol}: {Decision} ({Confidence:F0}% confidence)",
                    symbol, rec.Decision, rec.Confidence * 100);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scanning {Symbol}", symbol);
                result.Errors.Add($"{symbol}: {ex.Message}");
            }
        }

        return result;
    }
}