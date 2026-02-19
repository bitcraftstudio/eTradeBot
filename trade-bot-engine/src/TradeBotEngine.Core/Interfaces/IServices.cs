using TradeBotEngine.Core.Models;
using TradeBotEngine.Core.Models.ETrade;

namespace TradeBotEngine.Core.Interfaces;

/// <summary>
/// eTrade broker integration service
/// </summary>
public interface IETradeService
{
    // Authentication
    Task<string> GetAuthorizationUrlAsync();
    Task<ETradeOAuthTokens> CompleteAuthorizationAsync(string verifier);
    Task<bool> RefreshTokensAsync();
    Task<bool> IsAuthenticatedAsync();
    
    // Accounts
    Task<List<ETradeAccount>> GetAccountsAsync();
    Task<ETradeAccountBalance> GetAccountBalanceAsync(string accountId);
    Task<List<ETradePosition>> GetPositionsAsync(string accountId);
    
    // Orders
    Task<ETradeOrderResponse> PlaceOrderAsync(ETradeOrderRequest request);
    Task<ETradeOrderResponse> PreviewOrderAsync(ETradeOrderRequest request);
    Task<bool> CancelOrderAsync(string accountId, long orderId);
    Task<List<ETradeOrder>> GetOrdersAsync(string accountId);
    Task<ETradeOrder?> GetOrderAsync(string accountId, long orderId);
    
    // Market Data
    Task<ETradeQuote> GetQuoteAsync(string symbol);
    Task<List<ETradeQuote>> GetQuotesAsync(IEnumerable<string> symbols);
}

/// <summary>
/// AI-powered analysis service
/// </summary>
public interface IAIAnalysisService
{
    Task<MarketAnalysis> AnalyzeStockAsync(string symbol);
    Task<TradeRecommendation> GetTradeRecommendationAsync(
        string symbol, 
        decimal currentPrice, 
        TechnicalIndicators indicators, 
        NewsSentimentResult sentiment,
        RiskProfile riskProfile);
    Task<NewsSentimentResult> AnalyzeNewsSentimentAsync(string symbol, List<NewsArticle> articles);
    Task<TechnicalAnalysisResult> AnalyzeTechnicalsAsync(
        string symbol, 
        decimal currentPrice, 
        TechnicalIndicators indicators);
    Task<LearningAnalysis> ExtractLearningsAsync(List<Trade> trades);
}

/// <summary>
/// Market data service
/// </summary>
public interface IMarketDataService
{
    Task<StockQuote> GetQuoteAsync(string symbol);
    Task<List<StockQuote>> GetQuotesAsync(IEnumerable<string> symbols);
    Task<List<CandleData>> GetHistoricalDataAsync(string symbol, int days);
    Task<TechnicalIndicators> CalculateIndicatorsAsync(string symbol, List<CandleData> candles);
    Task CacheMarketDataAsync(string symbol, List<CandleData> candles);
    Task<List<MarketData>> GetCachedDataAsync(string symbol, int days);
}

/// <summary>
/// Trading service
/// </summary>
public interface ITradingService
{
    Task<TradeResult> ExecuteTradeAsync(ExecuteTradeRequest request);
    Task<List<Trade>> GetAllTradesAsync();
    Task<List<Trade>> GetOpenTradesAsync();
    Task<List<Trade>> GetClosedTradesAsync();
    Task<Trade?> GetTradeAsync(string tradeId);
    Task<bool> UpdateTradeAsync(Trade trade);
}

/// <summary>
/// Portfolio management service
/// </summary>
public interface IPortfolioService
{
    Task<PortfolioSummary> GetPortfolioSummaryAsync();
    Task<List<Position>> GetOpenPositionsAsync();
    Task<Position?> GetPositionAsync(string symbol);
    Task<Position> CreatePositionAsync(Position position);
    Task<Position> UpdatePositionAsync(Position position);
    Task<bool> ClosePositionAsync(string symbol);
    Task SyncWithETradeAsync();
}

/// <summary>
/// Risk management service
/// </summary>
public interface IRiskManagementService
{
    RiskCalculation CalculatePositionSize(
        string symbol, 
        decimal currentPrice, 
        decimal portfolioValue, 
        decimal cashAvailable,
        RiskProfile? riskProfile = null);
    
    (bool Allowed, string? Reason) CanOpenPosition(int currentOpenPositions, RiskProfile? riskProfile = null);
    (bool Meets, string? Reason) MeetsRiskRewardRequirements(decimal riskRewardRatio, RiskProfile? riskProfile = null);
    decimal CalculateTrailingStop(decimal entryPrice, decimal currentPrice, decimal currentStopLoss, RiskProfile? riskProfile = null);
    (bool ShouldTake, decimal? Percentage) ShouldTakePartialProfits(decimal entryPrice, decimal currentPrice, RiskProfile? riskProfile = null);
}

/// <summary>
/// Learning service
/// </summary>
public interface ILearningService
{
    Task<LearningReview> PerformDailyReviewAsync(int days = 7);
    Task<List<LearningInsight>> GetAllInsightsAsync();
    Task<LearningInsight?> GetLatestInsightAsync();
    Task<PerformanceMetrics> GetPerformanceMetricsAsync();
    Task<StrategyWeights> GetCurrentStrategyWeightsAsync();
    Task UpdateStrategyWeightsAsync(StrategyWeights weights);
}

/// <summary>
/// Stock hunter/discovery service
/// </summary>
public interface IStockHunterService
{
    Task<StockHunterResult> HuntStocksAsync(StockHunterFilters? filters = null);
    Task<List<DiscoveredStock>> GetDiscoveredStocksAsync();
    Task<bool> AddToWatchlistAsync(string symbol);
    StockHunterConfig GetConfig();
    void UpdateConfig(StockHunterConfig config);
}

/// <summary>
/// News service
/// </summary>
public interface INewsService
{
    Task<List<NewsArticle>> GetRecentNewsAsync(string symbol, int days = 7);
    Task<NewsArticle> SaveNewsArticleAsync(NewsArticle article);
    List<NewsArticle> GetMockNews(string symbol);
}

/// <summary>
/// Repository for trades
/// </summary>
public interface ITradeRepository
{
    Task<Trade?> GetByIdAsync(string id);
    Task<Trade?> GetByTradeIdAsync(string tradeId);
    Task<List<Trade>> GetAllAsync();
    Task<List<Trade>> GetByStatusAsync(TradeStatus status);
    Task<List<Trade>> GetByDateRangeAsync(DateTime start, DateTime end);
    Task<Trade> CreateAsync(Trade trade);
    Task<Trade> UpdateAsync(Trade trade);
    Task<bool> DeleteAsync(string id);
    Task<int> GetNextTradeNumberAsync();
}

/// <summary>
/// Repository for positions
/// </summary>
public interface IPositionRepository
{
    Task<Position?> GetByIdAsync(string id);
    Task<Position?> GetBySymbolAsync(string symbol);
    Task<List<Position>> GetAllAsync();
    Task<List<Position>> GetByStatusAsync(PositionStatus status);
    Task<Position> CreateAsync(Position position);
    Task<Position> UpdateAsync(Position position);
    Task<bool> DeleteAsync(string id);
}

/// <summary>
/// Repository for learning insights
/// </summary>
public interface ILearningInsightRepository
{
    Task<LearningInsight?> GetByIdAsync(string id);
    Task<LearningInsight?> GetLatestAsync();
    Task<List<LearningInsight>> GetAllAsync();
    Task<List<LearningInsight>> GetByDateRangeAsync(DateTime start, DateTime end);
    Task<LearningInsight> CreateAsync(LearningInsight insight);
    Task<bool> DeleteAsync(string id);
}

/// <summary>
/// Repository for market data
/// </summary>
public interface IMarketDataRepository
{
    Task<List<MarketData>> GetBySymbolAsync(string symbol, int days);
    Task<MarketData?> GetBySymbolAndDateAsync(string symbol, DateTime date);
    Task<MarketData> UpsertAsync(MarketData data);
    Task<bool> DeleteOldDataAsync(int daysToKeep);
}

/// <summary>
/// Repository for discovered stocks
/// </summary>
public interface IDiscoveredStockRepository
{
    Task<DiscoveredStock?> GetBySymbolAsync(string symbol);
    Task<List<DiscoveredStock>> GetAllAsync();
    Task<List<DiscoveredStock>> GetWatchlistAsync();
    Task<DiscoveredStock> CreateAsync(DiscoveredStock stock);
    Task<DiscoveredStock> UpdateAsync(DiscoveredStock stock);
    Task<bool> DeleteAsync(string id);
}

// DTOs for service methods
public class MarketAnalysis
{
    public string Symbol { get; set; } = string.Empty;
    public decimal CurrentPrice { get; set; }
    public TechnicalAnalysisResult TechnicalAnalysis { get; set; } = new();
    public NewsSentimentResult Sentiment { get; set; } = new();
    public TradeRecommendation Recommendation { get; set; } = new();
}

public class TechnicalAnalysisResult
{
    public MarketTrend Trend { get; set; }
    public int Strength { get; set; }
    public List<string> Signals { get; set; } = new();
}

public class NewsSentimentResult
{
    public string Symbol { get; set; } = string.Empty;
    public List<AnalyzedArticle> Articles { get; set; } = new();
    public decimal OverallSentiment { get; set; }
    public string SentimentLabel { get; set; } = "NEUTRAL";
}

public class AnalyzedArticle
{
    public string Title { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public DateTime PublishedDate { get; set; }
    public decimal Sentiment { get; set; }
    public decimal Relevance { get; set; }
    public string? Summary { get; set; }
}

public class TradeRecommendation
{
    public string Symbol { get; set; } = string.Empty;
    public TradeDecision Decision { get; set; }
    public decimal Confidence { get; set; }
    public string Reasoning { get; set; } = string.Empty;
    public RiskAssessment RiskAssessment { get; set; }
    public decimal ExpectedReturn { get; set; }
    public decimal MaxRisk { get; set; }
    public int TechnicalScore { get; set; }
    public decimal SentimentScore { get; set; }
    public int MomentumScore { get; set; }
    public decimal? SuggestedEntryPrice { get; set; }
    public decimal? SuggestedStopLoss { get; set; }
    public decimal? SuggestedTakeProfit { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class ExecuteTradeRequest
{
    public string Symbol { get; set; } = string.Empty;
    public TradeType Type { get; set; }
    public int? Quantity { get; set; }
    public decimal? Price { get; set; }
    public decimal? StopLoss { get; set; }
    public decimal? TakeProfit { get; set; }
    public string? AccountId { get; set; }
}

public class TradeResult
{
    public bool Success { get; set; }
    public string? TradeId { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public TradeType Type { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal TotalCost { get; set; }
    public decimal? StopLoss { get; set; }
    public decimal? TakeProfit { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? ErrorCode { get; set; }
}

public class RiskCalculation
{
    public string Symbol { get; set; } = string.Empty;
    public decimal CurrentPrice { get; set; }
    public string RiskProfile { get; set; } = string.Empty;
    public decimal PortfolioValue { get; set; }
    public decimal CashAvailable { get; set; }
    public int RecommendedQuantity { get; set; }
    public int MaxQuantity { get; set; }
    public decimal PositionValue { get; set; }
    public decimal CapitalRisked { get; set; }
    public decimal StopLossPrice { get; set; }
    public decimal TakeProfitPrice { get; set; }
    public decimal RiskRewardRatio { get; set; }
}

public class LearningReview
{
    public DateTime Date { get; set; }
    public int TradesReviewed { get; set; }
    public int SuccessfulTrades { get; set; }
    public int FailedTrades { get; set; }
    public decimal WinRate { get; set; }
    public decimal TotalProfitLoss { get; set; }
    public decimal AvgReturn { get; set; }
    public List<PatternInsight> Patterns { get; set; } = new();
    public LearningInsights Insights { get; set; } = new();
    public object? StrategyAdjustments { get; set; }
}

public class LearningInsights
{
    public List<string> SuccessFactors { get; set; } = new();
    public List<string> FailureFactors { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
}

public class LearningAnalysis
{
    public List<string> SuccessfulPatterns { get; set; } = new();
    public List<string> FailedPatterns { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
    public string KeyInsights { get; set; } = string.Empty;
    public object? StrategyAdjustments { get; set; }
}

public class PerformanceMetrics
{
    public int TotalTrades { get; set; }
    public decimal WinRate { get; set; }
    public decimal AvgReturn { get; set; }
    public decimal TotalProfitLoss { get; set; }
    public decimal AvgHoldingDays { get; set; }
    public TradeInfo BestTrade { get; set; } = new();
    public TradeInfo WorstTrade { get; set; } = new();
}

public class TradeInfo
{
    public string TradeId { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public decimal Return { get; set; }
}

public class StockHunterResult
{
    public int TotalFound { get; set; }
    public int Filtered { get; set; }
    public List<DiscoveredStock> Recommendations { get; set; } = new();
    public HunterSummary Summary { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class HunterSummary
{
    public decimal AvgSmartScore { get; set; }
    public decimal AvgUpside { get; set; }
    public List<string> SectorsRepresented { get; set; } = new();
    public DiscoveredStock? TopPick { get; set; }
}

public class StockHunterFilters
{
    public int? MinSmartScore { get; set; }
    public int? MaxSmartScore { get; set; }
    public decimal? MinMarketCap { get; set; }
    public decimal? MinAnalystRating { get; set; }
    public decimal? MinUpside { get; set; }
    public int? Limit { get; set; }
    public bool? RequiredHedgeFundActivity { get; set; }
    public bool? RequiredInsiderBuying { get; set; }
    public List<string>? ExcludeSymbols { get; set; }
}

public class StockHunterConfig
{
    public bool Enabled { get; set; }
    public bool RunDaily { get; set; }
    public string RunTime { get; set; } = "09:30";
    public bool AutoAddToWatchlist { get; set; }
    public int MaxDiscoveries { get; set; } = 10;
    public StockHunterFilters Filters { get; set; } = new();
}

// ─── Scheduler Config ─────────────────────────────────────────────────────────

/// <summary>
/// Repository for scheduler configuration (persisted)
/// </summary>
public interface ISchedulerConfigRepository
{
    Task<SchedulerConfig> GetConfigAsync();
    Task<SchedulerConfig> SaveConfigAsync(SchedulerConfig config);
    Task<SchedulerConfig> ResetConfigAsync();
}

/// <summary>
/// Scheduler configuration - persisted in CosmosDB
/// </summary>
public class SchedulerConfig
{
    public string Id { get; set; } = "scheduler-config"; // Singleton document
    
    // Master scheduler toggle
    public bool SchedulerEnabled { get; set; } = true;
    
    // Automation toggles
    public bool MorningScanEnabled { get; set; } = true;
    public bool PositionMonitoringEnabled { get; set; } = true;
    public bool DailyLearningEnabled { get; set; } = true;
    public bool AutoTradeEnabled { get; set; } = false;
    
    // Schedule settings
    public string MorningScanTime { get; set; } = "09:30";
    public int PositionCheckInterval { get; set; } = 15; // minutes
    
    // Trading settings - EMPTY WATCHLIST BY DEFAULT
    public List<string> Watchlist { get; set; } = new();
    public int MaxDailyTrades { get; set; } = 5;
    public decimal MinConfidence { get; set; } = 0.7m;
    
    // Timestamps
    public DateTime? LastScanTime { get; set; }
    public DateTime? LastMonitorTime { get; set; }
    public DateTime? LastLearningTime { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Partial update for scheduler config
/// </summary>
public class SchedulerConfigUpdate
{
    public bool? SchedulerEnabled { get; set; }
    public bool? MorningScanEnabled { get; set; }
    public bool? PositionMonitoringEnabled { get; set; }
    public bool? DailyLearningEnabled { get; set; }
    public bool? AutoTradeEnabled { get; set; }
    public string? MorningScanTime { get; set; }
    public int? PositionCheckInterval { get; set; }
    public List<string>? Watchlist { get; set; }
    public int? MaxDailyTrades { get; set; }
    public decimal? MinConfidence { get; set; }
}
