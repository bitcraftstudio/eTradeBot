using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using System.Linq;
using TradeBotEngine.Core.Interfaces;

namespace TradeBotEngine.Functions.Endpoints;

public class Trading
{
    private readonly ILogger<Trading> _logger;
    private readonly ITradingService _tradingService;
    private readonly IPortfolioService _portfolioService;
    private readonly IPositionMonitorService _positionMonitor;
    private readonly IMarketDataService _marketDataService;

    public Trading(
        ITradingService tradingService, 
        IPortfolioService portfolioService,
        IPositionMonitorService positionMonitor,
        IMarketDataService marketDataService,
        ILogger<Trading> logger)
    {
        _tradingService = tradingService;
        _portfolioService = portfolioService;
        _positionMonitor = positionMonitor;
        _marketDataService = marketDataService;
        _logger = logger;
    }

    /// <summary>POST /api/trading/execute</summary>
    [Function("ExecuteTrade")]
    public async Task<HttpResponseData> ExecuteTrade([HttpTrigger(AuthorizationLevel.Function, "post", Route = "trading/execute")] HttpRequestData req)
    {
        try
        {
            var body = await req.ReadAsStringAsync();
            if (string.IsNullOrEmpty(body))
                return await HttpUtils.JsonOther(req, HttpStatusCode.BadRequest, "Request body required");

            var request = JsonSerializer.Deserialize<ExecuteTradeRequest>(body, HttpUtils.JsonInOpts);
            if (request == null || string.IsNullOrEmpty(request.Symbol))
                return await HttpUtils.JsonOther(req, HttpStatusCode.BadRequest, "Symbol is required");

            var result = await _tradingService.ExecuteTradeAsync(request);
            return await HttpUtils.JsonCustom(req, result.Success ? HttpStatusCode.OK : HttpStatusCode.UnprocessableEntity, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing trade");
            return await HttpUtils.JsonOther(req, HttpStatusCode.InternalServerError, ex.Message);
        }
    }

    /// <summary>GET /api/trading/trades</summary>
    [Function("GetAllTrades")]
    public async Task<HttpResponseData> GetAllTrades([HttpTrigger(AuthorizationLevel.Function, "get", Route = "trading/trades")] HttpRequestData req)
    {
        var trades = await _tradingService.GetAllTradesAsync();
        return await HttpUtils.JsonOk(req, trades);
    }

    /// <summary>GET /api/trading/trades/open</summary>
    [Function("GetOpenTrades")]
    public async Task<HttpResponseData> GetOpenTrades([HttpTrigger(AuthorizationLevel.Function, "get", Route = "trading/trades/open")] HttpRequestData req)
    {
        var trades = await _tradingService.GetOpenTradesAsync();
        return await HttpUtils.JsonOk(req, trades);
    }

    /// <summary>GET /api/trading/trades/closed</summary>
    [Function("GetClosedTrades")]
    public async Task<HttpResponseData> GetClosedTrades([HttpTrigger(AuthorizationLevel.Function, "get", Route = "trading/trades/closed")] HttpRequestData req)
    {
        var trades = await _tradingService.GetClosedTradesAsync();
        return await HttpUtils.JsonOk(req, trades);
    }

    /// <summary>GET /api/trading/trades/{tradeId}</summary>
    [Function("GetTrade")]
    public async Task<HttpResponseData> GetTrade([HttpTrigger(AuthorizationLevel.Function, "get", Route = "trading/trades/{tradeId}")] HttpRequestData req, string tradeId)
    {
        var trade = await _tradingService.GetTradeAsync(tradeId);
        if (trade == null) return await HttpUtils.JsonOther(req, HttpStatusCode.NotFound, $"Trade {tradeId} not found");
        return await HttpUtils.JsonOk(req, trade);
    }

    /// <summary>GET /api/portfolio/summary</summary>
    [Function("GetPortfolioSummary")]
    public async Task<HttpResponseData> GetPortfolioSummary([HttpTrigger(AuthorizationLevel.Function, "get", Route = "portfolio/summary")] HttpRequestData req)
    {
        var summary = await _portfolioService.GetPortfolioSummaryAsync();
        return await HttpUtils.JsonOk(req, summary);
    }

    /// <summary>GET /api/portfolio/positions</summary>
    [Function("GetPositions")]
    public async Task<HttpResponseData> GetPositions([HttpTrigger(AuthorizationLevel.Function, "get", Route = "portfolio/positions")] HttpRequestData req)
    {
        var positions = await _portfolioService.GetOpenPositionsAsync();
        return await HttpUtils.JsonOk(req, positions);
    }

    /// <summary>POST /api/portfolio/sync - Sync with live eTrade account</summary>
    [Function("SyncPortfolio")]
    public async Task<HttpResponseData> SyncPortfolio([HttpTrigger(AuthorizationLevel.Function, "post", Route = "portfolio/sync")] HttpRequestData req)
    {
        await _portfolioService.SyncWithETradeAsync();
        var positions = await _portfolioService.GetOpenPositionsAsync();
        return await HttpUtils.JsonOk(req, new { message = "Portfolio synced", positions });
    }

    /// <summary>GET /api/portfolio/positions/sell-signals - Check for sell signals on all positions</summary>
    [Function("GetSellSignals")]
    public async Task<HttpResponseData> GetSellSignals([HttpTrigger(AuthorizationLevel.Function, "get", Route = "portfolio/positions/sell-signals")] HttpRequestData req)
    {
        try
        {
            // Update positions first
            await _positionMonitor.UpdateAllPositionsAsync();
            
            // Check for sell signals
            var signals = await _positionMonitor.CheckSellSignalsAsync();
            
            var result = new
            {
                TotalSignals = signals.Count,
                Signals = signals.Select(s => new
                {
                    s.Symbol,
                    ShouldSell = true,
                    s.Reason,
                    s.Confidence,
                    CurrentGain = s.CurrentGainPercent,
                    PeakGain = s.PeakGainPercent,
                    WeeklyTrend = s.WeeklyTrend.ToString()
                }).ToList(),
                CheckedAt = DateTime.UtcNow
            };

            return await HttpUtils.JsonOk(req, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking sell signals");
            return await HttpUtils.JsonOther(req, HttpStatusCode.InternalServerError, ex.Message);
        }
    }

    /// <summary>GET /api/portfolio/positions/{symbol}/analysis - Get detailed analysis for a position</summary>
    [Function("GetPositionAnalysis")]
    public async Task<HttpResponseData> GetPositionAnalysis([HttpTrigger(AuthorizationLevel.Function, "get", Route = "portfolio/positions/{symbol}/analysis")] HttpRequestData req, string symbol)
    {
        try
        {
            symbol = symbol.ToUpper();
            
            // Get position
            var position = await _portfolioService.GetPositionAsync(symbol);
            if (position == null)
            {
                return await HttpUtils.JsonOther(req, HttpStatusCode.NotFound, $"No position found for {symbol}");
            }

            // Get current quote
            var quote = await _marketDataService.GetQuoteAsync(symbol);
            
            // Update position with current price
            position.CurrentPrice = quote.Price;
            position.UnrealizedPnL = (quote.Price - position.EntryPrice) * position.Quantity;
            position.UnrealizedPnLPercent = position.EntryPrice > 0 
                ? ((quote.Price - position.EntryPrice) / position.EntryPrice) * 100 
                : 0;

            // Check sell signal for this position
            var signals = await _positionMonitor.CheckSellSignalsAsync();
            var sellSignal = signals.FirstOrDefault(s => s.Symbol == symbol);

            var analysis = new
            {
                Symbol = symbol,
                Position = new
                {
                    position.Symbol,
                    position.Quantity,
                    position.EntryPrice,
                    position.CurrentPrice,
                    position.PeakPrice,
                    position.StopLoss,
                    position.TakeProfit,
                    position.UnrealizedPnL,
                    position.UnrealizedPnLPercent,
                    position.DaysHeld,
                    position.EntryDate
                },
                SellSignal = sellSignal != null ? new
                {
                    ShouldSell = true,
                    sellSignal.Reason,
                    sellSignal.Confidence,
                    CurrentGain = sellSignal.CurrentGainPercent,
                    PeakGain = sellSignal.PeakGainPercent,
                    WeeklyTrend = sellSignal.WeeklyTrend.ToString()
                } : new
                {
                    ShouldSell = false,
                    Reason = "Position performing within expected parameters",
                    Confidence = 0.5m,
                    CurrentGain = position.UnrealizedPnLPercent,
                    PeakGain = position.PeakPrice > 0 && position.EntryPrice > 0
                        ? ((position.PeakPrice - position.EntryPrice) / position.EntryPrice) * 100
                        : 0m,
                    WeeklyTrend = "NEUTRAL"
                },
                DailySnapshots = position.DailySnapshots?.Select(s => new
                {
                    s.Date,
                    s.Price,
                    GainPercent = position.EntryPrice > 0 
                        ? ((s.Price - position.EntryPrice) / position.EntryPrice) * 100 
                        : 0
                }).Cast<object>().ToList() ?? new List<object>(),
                AnalyzedAt = DateTime.UtcNow
            };

            return await HttpUtils.JsonOk(req, analysis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing position for {Symbol}", symbol);
            return await HttpUtils.JsonOther(req, HttpStatusCode.InternalServerError, ex.Message);
        }
    }
}
