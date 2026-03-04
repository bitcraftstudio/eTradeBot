using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TradeBot.Core.Interfaces.Repository;
using TradeBot.Core.Models;
using TradeBot.Core.Models.Criteria;

namespace TradeBot.Engine.Repositories.Cosmos
{
    public class StocksRepository : RepositoryBase, IStockRepository
    {
        public StocksRepository(
            IConfiguration config,
            ILogger<StocksRepository> logger
        ) : base(config, logger, "stocks", "/symbol") { }

        public async Task<Stock> CreateAsync(Stock stock)
        {
            var result = await Container.CreateItemAsync<Stock>(stock, new PartitionKey(stock.Id));
            return result.Resource;
        }

        public async Task<bool> DeleteAsync(string symbol)
        {
            try { await Container.DeleteItemAsync<Stock>(symbol, new PartitionKey(symbol)); return true; }
            catch { return false; }
        }

        public async IAsyncEnumerable<Stock> GetBySearchAsync(StockCriteria criteria)
        {
            using var stockIterator = Container.GetItemLinqQueryable<Stock>(true)
                .Where(s =>
                    (!criteria.MarketCapMin.HasValue || s.MarketCap >= criteria.MarketCapMin.Value) &&
                    (!criteria.MarketCapMax.HasValue || s.MarketCap <= criteria.MarketCapMax.Value) &&
                    (criteria.Sectors == null || criteria.Sectors.Contains(s.Sector)) &&
                    (!criteria.PriceCapMin.HasValue || s.Price >= criteria.PriceCapMin.Value) &&
                    (!criteria.PriceCapMax.HasValue || s.Price <= criteria.PriceCapMax.Value) &&
                    (!criteria.IsEtf.HasValue || s.IsEtf == criteria.IsEtf.Value) &&
                    (!criteria.MinSmartScore.HasValue || (s.SmartScore.HasValue && s.SmartScore.Value >= criteria.MinSmartScore.Value)) &&
                    (!criteria.IsExcluded.HasValue || s.IsExcluded == criteria.IsExcluded.Value)
                ).ToFeedIterator();

            while (stockIterator.HasMoreResults)
            {
                var response = await stockIterator.ReadNextAsync();
                foreach (var item in response)
                {
                    yield return item;
                }
            }
        }

        public async Task<Stock?> GetBySymbolAsync(string symbol)
        {
            Stock stock = await Container.ReadItemAsync<Stock>(symbol, new PartitionKey(symbol));
            return stock;
        }

        public async Task<Stock> UpdateAsync(Stock stock)
        {
            stock.LastUpdated = DateTime.UtcNow;
            var result = await Container.UpsertItemAsync<Stock>(stock, new PartitionKey(stock.Id));
            return result.Resource;
        }

        public async Task<List<string>> GetExistingSymbolsAsync(IEnumerable<string> symbols)
        {
            var symbolSet = new HashSet<string>(symbols);
            using var iterator = Container.GetItemLinqQueryable<Stock>(true)
                .Where(s => symbolSet.Contains(s.Id))
                .Select(s => s.Id)
                .ToFeedIterator();

            var existing = new List<string>();
            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                existing.AddRange(response);
            }
            return existing;
        }

        public async Task<bool> BulkUpsertAsync(IEnumerable<Stock> stocks)
        {
            var tasks = stocks.Select(stock => Container.UpsertItemAsync(stock, new PartitionKey(stock.Id)));
            await Task.WhenAll(tasks);

            return true;
        }
    }
}
