using TradeBot.Core.Models.FMP;

namespace TradeBot.Core.Interfaces.Data
{
    public interface IFMPService
    {
        Task<List<StockScreenResult>> ScreenStocksAsync(StockScreenCriteria criteria);
        Task<CompanyProfile?> GetCompanyProfileAsync(string symbol);
        Task<StockQuote?> GetStockQuoteAsync(string symbol);
        Task<List<HistoricalGrade>> GetHistoricalGradesAsync(string symbol, int limit = 100);
        Task<List<StockGrade>> GetStockGradesAsync(string symbol);
        Task<StockGradeSummary?> GetStockGradeSummaryAsync(string symbol);
        Task<List<StockCandleStick>> GetHistoricalPricesAsync(string symbol, int days);
        Task<IncomeStatement?> GetIncomeStatementAsync(string symbol);
        Task<BalanceSheet?> GetBalanceSheetStatementAsync(string symbol);
        Task<CashFlowStatement?> GetCashFlowStatementAsync(string symbol);
        Task<KeyMetrics?> GetKeyMetricsAsync(string symbol);
        Task<RatingsSnapshot?> GetRatingsSnapshotAsync(string symbol);
        Task<PriceTargetConsensus?> GetPriceTargetConsensusAsync(string symbol);
        Task<bool> IsApiConfiguredAsync();
    }
}
