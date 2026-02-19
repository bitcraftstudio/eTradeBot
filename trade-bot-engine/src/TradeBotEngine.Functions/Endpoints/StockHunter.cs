using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Net;
using TradeBotEngine.Core.Interfaces;

namespace TradeBotEngine.Functions.Endpoints;

public class StockHunter
{
    private readonly ILogger<StockHunter> _logger;
    private readonly IStockHunterService _stockHunter;
    private readonly ILearningService _learningService;
    private readonly IConfiguration _configuration;

    public StockHunter(
        IStockHunterService stockHunter, 
        ILearningService learningService, 
        IConfiguration configuration,
        ILogger<StockHunter> logger)
    {
        _stockHunter = stockHunter;
        _learningService = learningService;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>GET /api/stocks/status - Get stock hunter status</summary>
    [Function("GetHunterStatus")]
    public async Task<HttpResponseData> GetHunterStatus([HttpTrigger(AuthorizationLevel.Function, "get", Route = "stocks/status")] HttpRequestData req)
    {
        var config = _stockHunter.GetConfig();
        var discovered = await _stockHunter.GetDiscoveredStocksAsync();
        
        var status = new
        {
            Enabled = config.Enabled,
            ApiConfigured = !string.IsNullOrEmpty(_configuration["TipRanks:ApiKey"]),
            TotalDiscovered = discovered.Count,
            WatchlistCount = discovered.Count(s => s.AddedToWatchlist),
            LastHuntTime = discovered.MaxBy(s => s.DiscoveredAt)?.DiscoveredAt,
            Config = config
        };
        
        return await HttpUtils.JsonOk(req, status);
    }

    /// <summary>POST /api/stocks/hunt - Hunt for top stocks</summary>
    [Function("HuntStocks")]
    public async Task<HttpResponseData> HuntStocks([HttpTrigger(AuthorizationLevel.Function, "post", Route = "stocks/hunt")] HttpRequestData req)
    {
        try
        {
            StockHunterFilters? filters = null;
            var body = await req.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(body))
            {
                filters = JsonSerializer.Deserialize<StockHunterFilters>(body, HttpUtils.JsonInOpts);
            }
                
            var result = await _stockHunter.HuntStocksAsync(filters);
            return await HttpUtils.JsonOk(req, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Stock hunt failed");
            return await HttpUtils.JsonOther(req, HttpStatusCode.InternalServerError, ex.Message);
        }
    }

    /// <summary>GET /api/stocks/discovered - Get all discovered stocks</summary>
    [Function("GetDiscoveredStocks")]
    public async Task<HttpResponseData> GetDiscoveredStocks([HttpTrigger(AuthorizationLevel.Function, "get", Route = "stocks/discovered")] HttpRequestData req)
    {
        var stocks = await _stockHunter.GetDiscoveredStocksAsync();
        return await HttpUtils.JsonOk(req, stocks);
    }

    /// <summary>POST /api/stocks/{symbol}/watchlist - Add stock to watchlist</summary>
    [Function("AddToWatchlist")]
    public async Task<HttpResponseData> AddToWatchlist([HttpTrigger(AuthorizationLevel.Function, "post", Route = "stocks/{symbol}/watchlist")] HttpRequestData req, string symbol)
    {
        await _stockHunter.AddToWatchlistAsync(symbol.ToUpper());
        return await HttpUtils.JsonOk(req, new { message = $"{symbol.ToUpper()} added to watchlist" });
    }

    /// <summary>GET /api/stocks/config - Get hunter config</summary>
    [Function("GetHunterConfig")]
    public async Task<HttpResponseData> GetHunterConfig([HttpTrigger(AuthorizationLevel.Function, "get", Route = "stocks/config")] HttpRequestData req)
    {
        var config = _stockHunter.GetConfig();
        return await HttpUtils.JsonOk(req, config);
    }

    /// <summary>PUT /api/stocks/config - Update hunter config</summary>
    [Function("UpdateHunterConfig")]
    public async Task<HttpResponseData> UpdateHunterConfig([HttpTrigger(AuthorizationLevel.Function, "put", Route = "stocks/config")] HttpRequestData req)
    {
        try
        {
            var body = await req.ReadAsStringAsync();
            var config = JsonSerializer.Deserialize<StockHunterConfig>(body ?? "{}", HttpUtils.JsonInOpts);
            if (config != null) _stockHunter.UpdateConfig(config);
            return await HttpUtils.JsonOk(req, new { message = "Config updated", config = _stockHunter.GetConfig() });
        }
        catch (Exception ex)
        {
            return await HttpUtils.JsonOther(req, HttpStatusCode.BadRequest, ex.Message);
        }
    }

    // ─── Learning ──────────────────────────────────────────────────────────────

    /// <summary>GET /api/learning/insights - Get all learning insights</summary>
    [Function("GetLearningInsights")]
    public async Task<HttpResponseData> GetLearningInsights([HttpTrigger(AuthorizationLevel.Function, "get", Route = "learning/insights")] HttpRequestData req)
    {
        var insights = await _learningService.GetAllInsightsAsync();
        return await HttpUtils.JsonOk(req, insights);
    }

    /// <summary>GET /api/learning/insights/latest - Get latest insight</summary>
    [Function("GetLatestInsight")]
    public async Task<HttpResponseData> GetLatestInsight([HttpTrigger(AuthorizationLevel.Function, "get", Route = "learning/insights/latest")] HttpRequestData req)
    {
        var insight = await _learningService.GetLatestInsightAsync();
        if (insight == null) 
        {
            // Return empty insight instead of 404
            return await HttpUtils.JsonOk(req, new {
                tradesReviewed = 0,
                winRate = 0,
                avgReturn = 0,
                totalProfitLoss = 0,
                successFactors = new List<string>(),
                failureFactors = new List<string>(),
                patterns = new List<object>()
            });
        }
        return await HttpUtils.JsonOk(req, insight);
    }

    /// <summary>GET /api/learning/performance - Get performance metrics</summary>
    [Function("GetPerformanceMetrics")]
    public async Task<HttpResponseData> GetPerformanceMetrics([HttpTrigger(AuthorizationLevel.Function, "get", Route = "learning/performance")] HttpRequestData req)
    {
        var metrics = await _learningService.GetPerformanceMetricsAsync();
        return await HttpUtils.JsonOk(req, metrics);
    }

    /// <summary>GET /api/learning/metrics - Get performance metrics (alias)</summary>
    [Function("GetLearningMetrics")]
    public async Task<HttpResponseData> GetLearningMetrics([HttpTrigger(AuthorizationLevel.Function, "get", Route = "learning/metrics")] HttpRequestData req)
    {
        var metrics = await _learningService.GetPerformanceMetricsAsync();
        return await HttpUtils.JsonOk(req, metrics);
    }

    /// <summary>GET /api/learning/strategy - Get current strategy weights</summary>
    [Function("GetStrategyWeights")]
    public async Task<HttpResponseData> GetStrategyWeights([HttpTrigger(AuthorizationLevel.Function, "get", Route = "learning/strategy")] HttpRequestData req)
    {
        var weights = await _learningService.GetCurrentStrategyWeightsAsync();
        return await HttpUtils.JsonOk(req, weights);
    }

    /// <summary>POST /api/learning/review - Perform learning review</summary>
    [Function("PerformLearningReview")]
    public async Task<HttpResponseData> PerformLearningReview([HttpTrigger(AuthorizationLevel.Function, "post", Route = "learning/review")] HttpRequestData req)
    {
        try
        {
            var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
            var days = int.TryParse(query["days"], out var d) ? d : 7;

            var review = await _learningService.PerformDailyReviewAsync(days);
            return await HttpUtils.JsonOk(req, review);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Learning review failed");
            return await HttpUtils.JsonOther(req, HttpStatusCode.InternalServerError, ex.Message);
        }
    }
}
