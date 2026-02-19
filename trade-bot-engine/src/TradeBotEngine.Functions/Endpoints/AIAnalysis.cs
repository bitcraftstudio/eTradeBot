using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using TradeBotEngine.Core.Interfaces;
using TradeBotEngine.Core.Models;

namespace TradeBotEngine.Functions.Endpoints;

public class AIAnalysis
{
    private readonly ILogger<AIAnalysis> _logger;
    private readonly IAIAnalysisService _aiService;
    private readonly IMarketDataService _marketDataService;
    private readonly IRiskManagementService _riskService;

    public AIAnalysis(IAIAnalysisService aiService, IMarketDataService marketDataService, IRiskManagementService riskService, ILogger<AIAnalysis> logger)
    {
        _logger = logger;
        _aiService = aiService;
        _marketDataService = marketDataService;
        _riskService = riskService;
    }

    [Function("AIAnalysis")]
    public async Task<HttpResponseData> AnalyzeStock([HttpTrigger(AuthorizationLevel.Function, "get", Route = "analysis/{symbol}")] HttpRequestData req, string symbol)
    {
        try
        {
            symbol = symbol.ToUpper();
            _logger.LogInformation("Full AI analysis requested for {Symbol}", symbol);
            var analysis = await _aiService.AnalyzeStockAsync(symbol);
            return await HttpUtils.JsonOk(req, analysis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Analysis failed for {Symbol}", symbol);
            return await HttpUtils.JsonOther(req, HttpStatusCode.InternalServerError, ex.Message);
        }
    }

    [Function("GetQuote")]
    public async Task<HttpResponseData> GetQuote([HttpTrigger(AuthorizationLevel.Function, "get", Route = "analysis/{symbol}/quote")] HttpRequestData req, string symbol)
    {
        try
        {
            var quote = await _marketDataService.GetQuoteAsync(symbol.ToUpper());
            return await HttpUtils.JsonOk(req, quote);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Quote failed for {Symbol}", symbol);
            return await HttpUtils.JsonOther(req, HttpStatusCode.InternalServerError, ex.Message);
        }
    }

    /// <summary>GET /api/analysis/{symbol}/indicators - Technical indicators</summary>
    [Function("GetIndicators")]
    public async Task<HttpResponseData> GetIndicators([HttpTrigger(AuthorizationLevel.Function, "get", Route = "analysis/{symbol}/indicators")] HttpRequestData req, string symbol)
    {
        try
        {
            symbol = symbol.ToUpper();
            var candles = await _marketDataService.GetHistoricalDataAsync(symbol, 60);
            var indicators = await _marketDataService.CalculateIndicatorsAsync(symbol, candles);
            return await HttpUtils.JsonOk(req, indicators);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Indicators failed for {Symbol}", symbol);
            return await HttpUtils.JsonOther(req, HttpStatusCode.InternalServerError, ex.Message);
        }
    }

    /// <summary>GET /api/analysis/{symbol}/risk - Risk calculation for a position</summary>
    [Function("GetRiskCalculation")]
    public async Task<HttpResponseData> GetRiskCalculation([HttpTrigger(AuthorizationLevel.Function, "get", Route = "analysis/{symbol}/risk")] HttpRequestData req, string symbol)
    {
        try
        {
            symbol = symbol.ToUpper();
            var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
            var portfolioValue = decimal.TryParse(query["portfolioValue"], out var pv) ? pv : 10000m;
            var cashAvailable = decimal.TryParse(query["cash"], out var ca) ? ca : portfolioValue;
            var riskProfileStr = query["riskProfile"] ?? "Moderate";
            var riskProfile = Enum.TryParse<RiskProfile>(riskProfileStr, true, out var rp) ? rp : RiskProfile.Moderate;

            var quote = await _marketDataService.GetQuoteAsync(symbol);
            var calc = _riskService.CalculatePositionSize(symbol, quote.Price, portfolioValue, cashAvailable, riskProfile);

            return await HttpUtils.JsonOk(req, calc);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Risk calculation failed for {Symbol}", symbol);
            return await HttpUtils.JsonOther(req, HttpStatusCode.InternalServerError, ex.Message);
        }
    }

    /// <summary>GET /api/analysis/multi - Analyze multiple symbols at once</summary>
    [Function("AnalyzeMultiple")]
    public async Task<HttpResponseData> AnalyzeMultiple([HttpTrigger(AuthorizationLevel.Function, "get", Route = "analysis/multi")] HttpRequestData req)
    {
        try
        {
            var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
            var symbolsParam = query["symbols"] ?? "AAPL,MSFT,GOOGL";
            var symbols = symbolsParam.Split(',').Select(s => s.Trim().ToUpper()).ToArray();

            var tasks = symbols.Select(s => _aiService.AnalyzeStockAsync(s));
            var results = await Task.WhenAll(tasks);

            return await HttpUtils.JsonOk(req, results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Multi-analysis failed");
            return await HttpUtils.JsonOther(req, HttpStatusCode.InternalServerError, ex.Message);
        }
    }
}