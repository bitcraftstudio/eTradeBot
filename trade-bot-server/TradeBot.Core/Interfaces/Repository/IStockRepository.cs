using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using TradeBot.Core.Models;

namespace TradeBot.Core.Interfaces.Repository
{
    public interface IStockRepository
    {
        IAsyncEnumerable<Stock> GetBySearchAsync(Models.Criteria.StockCriteria criteria);
        Task<Stock?> GetBySymbolAsync(string symbol);
        Task<List<String>> GetExistingSymbolsAsync(IEnumerable<string> symbol);
        Task<Stock> CreateAsync(Stock stock);
        Task<Stock> UpdateAsync(Stock stock);
        Task<bool> DeleteAsync(string symbol);
        Task<bool> BulkUpsertAsync(IEnumerable<Stock> stocks);
    }
}
