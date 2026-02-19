using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TradeBotEngine.Core.Interfaces;
using TradeBotEngine.Core.Models;

namespace TradeBotEngine.Infrastructure.Services;

/// <summary>
/// Trading service - executes buy/sell orders via eTrade and manages trade records
/// </summary>
public class TradingService : ITradingService
{
    private readonly IETradeService _eTradeService;
    private readonly IMarketDataService _marketDataService;
    private readonly IAIAnalysisService _aiAnalysisService;
    private readonly IRiskManagementService _riskManagement;
    private readonly IPortfolioService _portfolioService;
    private readonly ITradeRepository _tradeRepo;
    private readonly IConfiguration _config;
    private readonly ILogger<TradingService> _logger;

    public TradingService(
        IETradeService eTradeService,
        IMarketDataService marketDataService,
        IAIAnalysisService aiAnalysisService,
        IRiskManagementService riskManagement,
        IPortfolioService portfolioService,
        ITradeRepository tradeRepo,
        IConfiguration config,
        ILogger<TradingService> logger)
    {
        _eTradeService = eTradeService;
        _marketDataService = marketDataService;
        _aiAnalysisService = aiAnalysisService;
        _riskManagement = riskManagement;
        _portfolioService = portfolioService;
        _tradeRepo = tradeRepo;
        _config = config;
        _logger = logger;
    }

    public async Task<TradeResult> ExecuteTradeAsync(ExecuteTradeRequest request)
    {
        var symbol = request.Symbol.ToUpper();
        _logger.LogInformation("=== Executing {Type} trade for {Symbol} ===", request.Type, symbol);

        try
        {
            // ── 1. Get current price ─────────────────────────────────────────────
            var quote = await _marketDataService.GetQuoteAsync(symbol);
            var currentPrice = request.Price ?? quote.Price;

            // ── 2. Get portfolio state ───────────────────────────────────────────
            var portfolio = await _portfolioService.GetPortfolioSummaryAsync();
            var cashAvailable = portfolio.CashBalance;

            // ── 3. Risk management ───────────────────────────────────────────────
            var riskProfileStr = _config["Trading:DefaultRiskProfile"] ?? "Moderate";
            var riskProfile = Enum.TryParse<RiskProfile>(riskProfileStr, true, out var rp) ? rp : RiskProfile.Moderate;

            var riskCalc = _riskManagement.CalculatePositionSize(symbol, currentPrice, portfolio.TotalValue, cashAvailable, riskProfile);
            var quantity = request.Quantity ?? riskCalc.RecommendedQuantity;

            if (quantity < 1)
                return Fail("INSUFFICIENT_FUNDS", symbol, request.Type, "Insufficient funds for minimum position size");

            // ── 4. Validate open position limits ─────────────────────────────────
            var openPositions = await _portfolioService.GetOpenPositionsAsync();
            var (canOpen, reason) = _riskManagement.CanOpenPosition(openPositions.Count, riskProfile);
            if (!canOpen && request.Type == TradeType.Buy)
                return Fail("MAX_POSITIONS_REACHED", symbol, request.Type, reason ?? "Max positions reached");

            // ── 5. Validate risk/reward ───────────────────────────────────────────
            var (meetsRR, rrReason) = _riskManagement.MeetsRiskRewardRequirements(riskCalc.RiskRewardRatio, riskProfile);
            if (!meetsRR && request.Type == TradeType.Buy)
                return Fail("RISK_REWARD_TOO_LOW", symbol, request.Type, rrReason ?? "R:R too low");

            var totalCost = quantity * currentPrice;

            if (request.Type == TradeType.Buy)
            {
                if (totalCost > cashAvailable)
                    return Fail("INSUFFICIENT_CASH", symbol, request.Type,
                        $"Need ${totalCost:F2}, available ${cashAvailable:F2}");

                return await ExecuteBuyAsync(symbol, quantity, currentPrice, riskCalc, request);
            }
            else
            {
                return await ExecuteSellAsync(symbol, quantity, currentPrice, request);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Trade execution failed for {Symbol}", symbol);
            return Fail("TRADE_EXECUTION_FAILED", symbol, request.Type, ex.Message);
        }
    }

    private async Task<TradeResult> ExecuteBuyAsync(
        string symbol, int quantity, decimal price, RiskCalculation riskCalc, ExecuteTradeRequest request)
    {
        // ── Place order with eTrade ───────────────────────────────────────────
        var accountId = request.AccountId ?? _config["ETrade:DefaultAccountId"] ?? "";
        decimal fillPrice = price;

        if (!string.IsNullOrEmpty(accountId))
        {
            var orderReq = new TradeBotEngine.Core.Models.ETrade.ETradeOrderRequest
            {
                AccountId = accountId,
                Symbol = symbol,
                OrderAction = TradeBotEngine.Core.Models.ETrade.ETradeOrderAction.Buy,
                OrderType = TradeBotEngine.Core.Models.ETrade.ETradeOrderType.Market,
                Duration = TradeBotEngine.Core.Models.ETrade.ETradeOrderDuration.Day,
                Quantity = quantity
            };

            var fill = await _eTradeService.PlaceOrderAsync(orderReq);
            if (!fill.Success)
                return Fail("ORDER_REJECTED", symbol, TradeType.Buy, fill.Message ?? "Order rejected by eTrade");

            fillPrice = fill.FilledPrice ?? price;
        }

        // ── Get AI analysis for context ───────────────────────────────────────
        var analysis = await TryGetAnalysis(symbol);
        var candles = await _marketDataService.GetHistoricalDataAsync(symbol, 60);
        var indicators = await _marketDataService.CalculateIndicatorsAsync(symbol, candles);

        // ── Build & save trade record ─────────────────────────────────────────
        var tradeNumber = await _tradeRepo.GetNextTradeNumberAsync();
        var tradeId = $"TRADE_{DateTime.UtcNow.Year}_{tradeNumber:D3}";
        var stopLoss = request.StopLoss ?? riskCalc.StopLossPrice;
        var takeProfit = request.TakeProfit ?? riskCalc.TakeProfitPrice;
        var totalCost = quantity * fillPrice;

        var trade = new Trade
        {
            TradeId = tradeId,
            Symbol = symbol,
            Type = TradeType.Buy,
            Quantity = quantity,
            EntryPrice = fillPrice,
            EntryDate = DateTime.UtcNow,
            StopLoss = stopLoss,
            TakeProfit = takeProfit,
            RiskRewardRatio = riskCalc.RiskRewardRatio,
            PositionSize = quantity,
            CapitalRisked = riskCalc.CapitalRisked,
            Status = TradeStatus.Open,
            ETradeAccountId = accountId,
            ContextAtEntry = new ContextAtEntry
            {
                TechnicalIndicators = indicators,
                MarketConditions = new MarketConditions { MarketTrend = analysis?.TechnicalAnalysis.Trend ?? MarketTrend.Neutral },
                NewsHeadlines = analysis?.Sentiment.Articles
                    .Select(a => new NewsHeadline { Title = a.Title, Sentiment = a.Sentiment, Source = a.Source, Date = a.PublishedDate })
                    .ToList() ?? new List<NewsHeadline>()
            },
            AIReasoning = MapAIReasoning(analysis)
        };

        await _tradeRepo.CreateAsync(trade);

        // ── Create position record ────────────────────────────────────────────
        await _portfolioService.CreatePositionAsync(new Position
        {
            TradeId = tradeId,
            Symbol = symbol,
            Quantity = quantity,
            EntryPrice = fillPrice,
            CurrentPrice = fillPrice,
            StopLoss = stopLoss,
            TakeProfit = takeProfit,
            Status = PositionStatus.Open,
            ETradeAccountId = accountId
        });

        _logger.LogInformation("✅ BUY {Symbol}: {Qty} shares @ ${Price:F2} | Trade: {TradeId}", symbol, quantity, fillPrice, tradeId);

        return new TradeResult
        {
            Success = true,
            TradeId = tradeId,
            Symbol = symbol,
            Type = TradeType.Buy,
            Quantity = quantity,
            Price = fillPrice,
            TotalCost = totalCost,
            StopLoss = stopLoss,
            TakeProfit = takeProfit,
            Message = $"BUY executed: {quantity} shares @ ${fillPrice:F2}",
            Timestamp = DateTime.UtcNow
        };
    }

    private async Task<TradeResult> ExecuteSellAsync(string symbol, int quantity, decimal price, ExecuteTradeRequest request)
    {
        var position = await _portfolioService.GetPositionAsync(symbol);
        if (position == null)
            return Fail("NO_POSITION_FOUND", symbol, TradeType.Sell, $"No open position found for {symbol}");

        var accountId = request.AccountId ?? position.ETradeAccountId ?? _config["ETrade:DefaultAccountId"] ?? "";
        decimal fillPrice = price;

        if (!string.IsNullOrEmpty(accountId))
        {
            var orderReq = new TradeBotEngine.Core.Models.ETrade.ETradeOrderRequest
            {
                AccountId = accountId,
                Symbol = symbol,
                OrderAction = TradeBotEngine.Core.Models.ETrade.ETradeOrderAction.Sell,
                OrderType = TradeBotEngine.Core.Models.ETrade.ETradeOrderType.Market,
                Duration = TradeBotEngine.Core.Models.ETrade.ETradeOrderDuration.Day,
                Quantity = quantity
            };

            var fill = await _eTradeService.PlaceOrderAsync(orderReq);
            fillPrice = fill.FilledPrice ?? price;
        }

        var proceeds = quantity * fillPrice;
        var profitLoss = proceeds - (position.EntryPrice * quantity);
        var profitLossPct = (profitLoss / (position.EntryPrice * quantity)) * 100;
        var holdingDays = (int)(DateTime.UtcNow - position.CreatedAt).TotalDays;

        // ── Update trade record ───────────────────────────────────────────────
        var trade = await _tradeRepo.GetByTradeIdAsync(position.TradeId);
        if (trade != null)
        {
            trade.ExitPrice = fillPrice;
            trade.ExitDate = DateTime.UtcNow;
            trade.Status = TradeStatus.Closed;
            trade.Outcome = new TradeOutcome
            {
                ProfitLoss = profitLoss,
                ProfitLossPercent = profitLossPct,
                HoldingDays = holdingDays,
                ExitReason = ExitReason.ManualExit
            };
            await _tradeRepo.UpdateAsync(trade);
        }

        await _portfolioService.ClosePositionAsync(symbol);

        _logger.LogInformation("✅ SELL {Symbol}: {Qty} shares @ ${Price:F2} | P/L: ${PnL:F2} ({Pct:F2}%)",
            symbol, quantity, fillPrice, profitLoss, profitLossPct);

        return new TradeResult
        {
            Success = true,
            Symbol = symbol,
            Type = TradeType.Sell,
            Quantity = quantity,
            Price = fillPrice,
            TotalCost = proceeds,
            Message = $"SELL executed: {quantity} shares @ ${fillPrice:F2} | P/L: ${profitLoss:F2}",
            Timestamp = DateTime.UtcNow
        };
    }

    public async Task<List<Trade>> GetAllTradesAsync() => await _tradeRepo.GetAllAsync();
    public async Task<List<Trade>> GetOpenTradesAsync() => await _tradeRepo.GetByStatusAsync(TradeStatus.Open);
    public async Task<List<Trade>> GetClosedTradesAsync() => await _tradeRepo.GetByStatusAsync(TradeStatus.Closed);
    public async Task<Trade?> GetTradeAsync(string tradeId) => await _tradeRepo.GetByTradeIdAsync(tradeId);
    public async Task<bool> UpdateTradeAsync(Trade trade) { await _tradeRepo.UpdateAsync(trade); return true; }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private async Task<MarketAnalysis?> TryGetAnalysis(string symbol)
    {
        try { return await _aiAnalysisService.AnalyzeStockAsync(symbol); }
        catch { return null; }
    }

    private static AIReasoning MapAIReasoning(MarketAnalysis? analysis) =>
        analysis == null ? new AIReasoning { Model = "none" } : new AIReasoning
        {
            Model = "claude-opus-4-5",
            Decision = analysis.Recommendation.Decision,
            Confidence = analysis.Recommendation.Confidence,
            Reasoning = analysis.Recommendation.Reasoning,
            RiskAssessment = analysis.Recommendation.RiskAssessment,
            ExpectedReturn = analysis.Recommendation.ExpectedReturn,
            MaxRisk = analysis.Recommendation.MaxRisk,
            TechnicalScore = analysis.Recommendation.TechnicalScore,
            SentimentScore = analysis.Recommendation.SentimentScore,
            MomentumScore = analysis.Recommendation.MomentumScore
        };

    private static TradeResult Fail(string code, string symbol, TradeType type, string message) =>
        new() { Success = false, Symbol = symbol, Type = type, Message = message, ErrorCode = code, Timestamp = DateTime.UtcNow };
}
