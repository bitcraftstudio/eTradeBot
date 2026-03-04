using System;
using System.Collections.Generic;
using System.Text;

namespace TradeBot.Core.Interfaces
{
    public interface IStockService
    {
        Task<bool> CollectStocksAsync(Models.FMP.StockScreenCriteria criteria);
        Task<bool> RefreshStockAsync(string symbol, bool enrich = true);
        Task<Models.Stock?> GetStockAsync(string symbol);


        //Task<Models.StockGradeSummary?> CollectGradeSummaryAsync(string symbol);
        //Task<Models.FMP.IncomeStatement?> CollectIncomeStatementAsync(string symbol);
        //Task<Models.FMP.BalanceSheet?> CollectBalanceSheetStatementAsync(string symbol);
        //Task<Models.FMP.CashFlowStatement?> CollectCashFlowStatementAsync(string symbol);
        //Task<Models.FMP.KeyMetrics?> CollectKeyMetricsAsync(string symbol);
        // Task<List<Models.StockCandleStick>> CollectHistoricalPricesAsync(string symbol);
        //Task<Models.StockGradeSummary?> GetGradeSummaryAsync(string symbol);
    }
}
