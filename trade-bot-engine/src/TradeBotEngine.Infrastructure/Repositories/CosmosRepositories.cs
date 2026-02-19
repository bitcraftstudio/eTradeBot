using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TradeBotEngine.Core.Interfaces;
using TradeBotEngine.Core.Models;

namespace TradeBotEngine.Infrastructure.Repositories;

/// <summary>
/// Base Cosmos DB repository - shared client and container initialization
/// </summary>
public abstract class CosmosRepositoryBase
{
    protected readonly Container Container;
    protected readonly ILogger Logger;

    protected CosmosRepositoryBase(IConfiguration config, ILogger logger, string containerName, string partitionKeyPath)
    {
        Logger = logger;
        var connectionString = config["Cosmos:ConnectionString"] ?? throw new InvalidOperationException("Cosmos:ConnectionString not configured");
        var dbName = config["Cosmos:DatabaseName"] ?? "TradeBotDB";

        var clientOptions = new CosmosClientOptions
        {
            SerializerOptions = new CosmosSerializationOptions
            {
                PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
            }
        };

        var client = new CosmosClient(connectionString, clientOptions);
        var db = client.GetDatabase(dbName);

        // Create container if it doesn't exist (idempotent)
        Container = db.CreateContainerIfNotExistsAsync(containerName, partitionKeyPath).GetAwaiter().GetResult();
    }
}

// ─────────────────────────────────────────────────────────────────────────────

/// <summary>Cosmos DB repository for Trade documents</summary>
public class CosmosTradeRepository : CosmosRepositoryBase, ITradeRepository
{
    public CosmosTradeRepository(IConfiguration config, ILogger<CosmosTradeRepository> logger)
        : base(config, logger, "trades", "/symbol") { }

    public async Task<Trade?> GetByIdAsync(string id)
    {
        try { return (await Container.ReadItemAsync<Trade>(id, new PartitionKey(id))).Resource; }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound) { return null; }
    }

    public async Task<Trade?> GetByTradeIdAsync(string tradeId)
    {
        var query = new QueryDefinition("SELECT * FROM c WHERE c.tradeId = @tradeId").WithParameter("@tradeId", tradeId);
        return await QueryFirstOrDefaultAsync<Trade>(query);
    }

    public async Task<List<Trade>> GetAllAsync()
    {
        var query = new QueryDefinition("SELECT * FROM c ORDER BY c.createdAt DESC");
        return await QueryListAsync<Trade>(query);
    }

    public async Task<List<Trade>> GetByStatusAsync(TradeStatus status)
    {
        var query = new QueryDefinition("SELECT * FROM c WHERE c.status = @status ORDER BY c.createdAt DESC")
            .WithParameter("@status", status.ToString());
        return await QueryListAsync<Trade>(query);
    }

    public async Task<List<Trade>> GetByDateRangeAsync(DateTime start, DateTime end)
    {
        var query = new QueryDefinition("SELECT * FROM c WHERE c.entryDate >= @start AND c.entryDate <= @end")
            .WithParameter("@start", start).WithParameter("@end", end);
        return await QueryListAsync<Trade>(query);
    }

    public async Task<Trade> CreateAsync(Trade trade)
    {
        trade.CreatedAt = DateTime.UtcNow;
        trade.UpdatedAt = DateTime.UtcNow;
        var result = await Container.CreateItemAsync(trade, new PartitionKey(trade.Symbol));
        return result.Resource;
    }

    public async Task<Trade> UpdateAsync(Trade trade)
    {
        trade.UpdatedAt = DateTime.UtcNow;
        var result = await Container.UpsertItemAsync(trade, new PartitionKey(trade.Symbol));
        return result.Resource;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        try { await Container.DeleteItemAsync<Trade>(id, new PartitionKey(id)); return true; }
        catch { return false; }
    }

    public async Task<int> GetNextTradeNumberAsync()
    {
        var query = new QueryDefinition("SELECT VALUE COUNT(1) FROM c");
        using var iter = Container.GetItemQueryIterator<int>(query);
        var response = await iter.ReadNextAsync();
        return response.Resource.FirstOrDefault() + 1;
    }

    private async Task<T?> QueryFirstOrDefaultAsync<T>(QueryDefinition query)
    {
        using var iter = Container.GetItemQueryIterator<T>(query);
        while (iter.HasMoreResults)
        {
            var page = await iter.ReadNextAsync();
            return page.Resource.FirstOrDefault();
        }
        return default;
    }

    private async Task<List<T>> QueryListAsync<T>(QueryDefinition query)
    {
        var results = new List<T>();
        using var iter = Container.GetItemQueryIterator<T>(query);
        while (iter.HasMoreResults)
        {
            var page = await iter.ReadNextAsync();
            results.AddRange(page.Resource);
        }
        return results;
    }
}

// ─────────────────────────────────────────────────────────────────────────────

/// <summary>Cosmos DB repository for Position documents</summary>
public class CosmosPositionRepository : CosmosRepositoryBase, IPositionRepository
{
    public CosmosPositionRepository(IConfiguration config, ILogger<CosmosPositionRepository> logger)
        : base(config, logger, "positions", "/symbol") { }

    public async Task<Position?> GetByIdAsync(string id)
    {
        try { return (await Container.ReadItemAsync<Position>(id, new PartitionKey(id))).Resource; }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound) { return null; }
    }

    public async Task<Position?> GetBySymbolAsync(string symbol)
    {
        var query = new QueryDefinition("SELECT * FROM c WHERE c.symbol = @symbol AND c.status = 'Open'")
            .WithParameter("@symbol", symbol);
        using var iter = Container.GetItemQueryIterator<Position>(query);
        while (iter.HasMoreResults)
        {
            var page = await iter.ReadNextAsync();
            return page.Resource.FirstOrDefault();
        }
        return null;
    }

    public async Task<List<Position>> GetAllAsync()
    {
        var results = new List<Position>();
        using var iter = Container.GetItemQueryIterator<Position>(new QueryDefinition("SELECT * FROM c"));
        while (iter.HasMoreResults) { var p = await iter.ReadNextAsync(); results.AddRange(p.Resource); }
        return results;
    }

    public async Task<List<Position>> GetByStatusAsync(PositionStatus status)
    {
        var query = new QueryDefinition("SELECT * FROM c WHERE c.status = @status")
            .WithParameter("@status", status.ToString());
        var results = new List<Position>();
        using var iter = Container.GetItemQueryIterator<Position>(query);
        while (iter.HasMoreResults) { var p = await iter.ReadNextAsync(); results.AddRange(p.Resource); }
        return results;
    }

    public async Task<Position> CreateAsync(Position position)
    {
        position.CreatedAt = DateTime.UtcNow;
        position.LastUpdated = DateTime.UtcNow;
        var result = await Container.CreateItemAsync(position, new PartitionKey(position.Symbol));
        return result.Resource;
    }

    public async Task<Position> UpdateAsync(Position position)
    {
        position.LastUpdated = DateTime.UtcNow;
        var result = await Container.UpsertItemAsync(position, new PartitionKey(position.Symbol));
        return result.Resource;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        try { await Container.DeleteItemAsync<Position>(id, new PartitionKey(id)); return true; }
        catch { return false; }
    }
}

// ─────────────────────────────────────────────────────────────────────────────

/// <summary>Cosmos DB repository for LearningInsight documents</summary>
public class CosmosLearningInsightRepository : CosmosRepositoryBase, ILearningInsightRepository
{
    public CosmosLearningInsightRepository(IConfiguration config, ILogger<CosmosLearningInsightRepository> logger)
        : base(config, logger, "learningInsights", "/id") { }

    public async Task<LearningInsight?> GetByIdAsync(string id)
    {
        try { return (await Container.ReadItemAsync<LearningInsight>(id, new PartitionKey(id))).Resource; }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound) { return null; }
    }

    public async Task<LearningInsight?> GetLatestAsync()
    {
        var query = new QueryDefinition("SELECT TOP 1 * FROM c ORDER BY c.createdAt DESC");
        using var iter = Container.GetItemQueryIterator<LearningInsight>(query);
        while (iter.HasMoreResults)
        {
            var page = await iter.ReadNextAsync();
            return page.Resource.FirstOrDefault();
        }
        return null;
    }

    public async Task<List<LearningInsight>> GetAllAsync()
    {
        var results = new List<LearningInsight>();
        using var iter = Container.GetItemQueryIterator<LearningInsight>(
            new QueryDefinition("SELECT * FROM c ORDER BY c.createdAt DESC"));
        while (iter.HasMoreResults) { var p = await iter.ReadNextAsync(); results.AddRange(p.Resource); }
        return results;
    }

    public async Task<List<LearningInsight>> GetByDateRangeAsync(DateTime start, DateTime end)
    {
        var query = new QueryDefinition("SELECT * FROM c WHERE c.date >= @start AND c.date <= @end")
            .WithParameter("@start", start).WithParameter("@end", end);
        var results = new List<LearningInsight>();
        using var iter = Container.GetItemQueryIterator<LearningInsight>(query);
        while (iter.HasMoreResults) { var p = await iter.ReadNextAsync(); results.AddRange(p.Resource); }
        return results;
    }

    public async Task<LearningInsight> CreateAsync(LearningInsight insight)
    {
        insight.CreatedAt = DateTime.UtcNow;
        var result = await Container.CreateItemAsync(insight, new PartitionKey(insight.Id));
        return result.Resource;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        try { await Container.DeleteItemAsync<LearningInsight>(id, new PartitionKey(id)); return true; }
        catch { return false; }
    }
}

// ─────────────────────────────────────────────────────────────────────────────

/// <summary>Cosmos DB repository for MarketData documents</summary>
public class CosmosMarketDataRepository : CosmosRepositoryBase, IMarketDataRepository
{
    public CosmosMarketDataRepository(IConfiguration config, ILogger<CosmosMarketDataRepository> logger)
        : base(config, logger, "marketData", "/symbol") { }

    public async Task<List<MarketData>> GetBySymbolAsync(string symbol, int days)
    {
        var since = DateTime.UtcNow.AddDays(-days);
        var query = new QueryDefinition("SELECT * FROM c WHERE c.symbol = @symbol AND c.date >= @since ORDER BY c.date ASC")
            .WithParameter("@symbol", symbol).WithParameter("@since", since);
        var results = new List<MarketData>();
        using var iter = Container.GetItemQueryIterator<MarketData>(query);
        while (iter.HasMoreResults) { var p = await iter.ReadNextAsync(); results.AddRange(p.Resource); }
        return results;
    }

    public async Task<MarketData?> GetBySymbolAndDateAsync(string symbol, DateTime date)
    {
        var dateStr = date.Date.ToString("yyyy-MM-dd");
        var query = new QueryDefinition("SELECT * FROM c WHERE c.symbol = @symbol AND STARTSWITH(c.date, @date)")
            .WithParameter("@symbol", symbol).WithParameter("@date", dateStr);
        using var iter = Container.GetItemQueryIterator<MarketData>(query);
        while (iter.HasMoreResults)
        {
            var page = await iter.ReadNextAsync();
            return page.Resource.FirstOrDefault();
        }
        return null;
    }

    public async Task<MarketData> UpsertAsync(MarketData data)
    {
        var result = await Container.UpsertItemAsync(data, new PartitionKey(data.Symbol));
        return result.Resource;
    }

    public async Task<bool> DeleteOldDataAsync(int daysToKeep)
    {
        var cutoff = DateTime.UtcNow.AddDays(-daysToKeep);
        var query = new QueryDefinition("SELECT c.id, c.symbol FROM c WHERE c.date < @cutoff")
            .WithParameter("@cutoff", cutoff);
        using var iter = Container.GetItemQueryIterator<dynamic>(query);
        while (iter.HasMoreResults)
        {
            var page = await iter.ReadNextAsync();
            foreach (var item in page.Resource)
                await Container.DeleteItemAsync<MarketData>((string)item.id, new PartitionKey((string)item.symbol));
        }
        return true;
    }
}

// ─────────────────────────────────────────────────────────────────────────────

/// <summary>Cosmos DB repository for SchedulerConfig (singleton document)</summary>
public class CosmosSchedulerConfigRepository : CosmosRepositoryBase, ISchedulerConfigRepository
{
    private const string CONFIG_ID = "scheduler-config";

    public CosmosSchedulerConfigRepository(IConfiguration config, ILogger<CosmosSchedulerConfigRepository> logger)
        : base(config, logger, "schedulerConfig", "/id") 
    {
        Logger.LogInformation("CosmosSchedulerConfigRepository initialized");
    }

    public async Task<SchedulerConfig> GetConfigAsync()
    {
        Logger.LogInformation("Getting scheduler config from Cosmos DB");
        
        try
        {
            var response = await Container.ReadItemAsync<SchedulerConfig>(CONFIG_ID, new PartitionKey(CONFIG_ID));
            var config = response.Resource;
            
            // Ensure watchlist is never null
            config.Watchlist ??= new List<string>();
            
            Logger.LogInformation("Loaded config: Watchlist={Count}, MinConfidence={MinConf}, MaxDailyTrades={MaxTrades}", 
                config.Watchlist.Count, config.MinConfidence, config.MaxDailyTrades);
            return config;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            Logger.LogInformation("No config found, creating default config with EMPTY watchlist");
            
            // Create default config with EMPTY watchlist
            var defaultConfig = new SchedulerConfig
            {
                Id = CONFIG_ID,
                MorningScanEnabled = true,
                PositionMonitoringEnabled = true,
                DailyLearningEnabled = true,
                AutoTradeEnabled = false,
                MorningScanTime = "09:30",
                PositionCheckInterval = 15,
                Watchlist = new List<string>(), // EMPTY!
                MaxDailyTrades = 5,
                MinConfidence = 0.7m,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            var created = await Container.CreateItemAsync(defaultConfig, new PartitionKey(CONFIG_ID));
            Logger.LogInformation("Created default config with empty watchlist");
            return created.Resource;
        }
    }

    public async Task<SchedulerConfig> SaveConfigAsync(SchedulerConfig config)
    {
        config.Id = CONFIG_ID;
        config.UpdatedAt = DateTime.UtcNow;
        
        // Ensure watchlist is a proper list (even if empty)
        config.Watchlist ??= new List<string>();
        
        // Remove duplicates and normalize to uppercase
        config.Watchlist = config.Watchlist
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s.Trim().ToUpperInvariant())
            .Distinct()
            .ToList();
        
        Logger.LogInformation("Saving config: Watchlist={Count} [{Items}], MinConfidence={MinConf}, MaxDailyTrades={MaxTrades}", 
            config.Watchlist.Count,
            string.Join(", ", config.Watchlist),
            config.MinConfidence,
            config.MaxDailyTrades);
        
        // Use UpsertItemAsync to create or replace the document
        var response = await Container.UpsertItemAsync(config, new PartitionKey(CONFIG_ID));
        
        Logger.LogInformation("Config saved. Result: Watchlist={Count}, MinConfidence={MinConf}", 
            response.Resource.Watchlist?.Count ?? 0,
            response.Resource.MinConfidence);
        
        return response.Resource;
    }

    /// <summary>Reset config to defaults (deletes existing and creates new)</summary>
    public async Task<SchedulerConfig> ResetConfigAsync()
    {
        Logger.LogInformation("Resetting scheduler config to defaults");
        
        try
        {
            await Container.DeleteItemAsync<SchedulerConfig>(CONFIG_ID, new PartitionKey(CONFIG_ID));
            Logger.LogInformation("Deleted existing config");
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            Logger.LogInformation("No existing config to delete");
        }
        
        // Get will create a new default config
        return await GetConfigAsync();
    }
}

// ─────────────────────────────────────────────────────────────────────────────

/// <summary>Cosmos DB repository for DiscoveredStock documents</summary>
public class CosmosDiscoveredStockRepository : CosmosRepositoryBase, IDiscoveredStockRepository
{
    public CosmosDiscoveredStockRepository(IConfiguration config, ILogger<CosmosDiscoveredStockRepository> logger)
        : base(config, logger, "discoveredStocks", "/symbol") { }

    public async Task<DiscoveredStock?> GetBySymbolAsync(string symbol)
    {
        var query = new QueryDefinition("SELECT TOP 1 * FROM c WHERE c.symbol = @symbol ORDER BY c.discoveredAt DESC")
            .WithParameter("@symbol", symbol);
        using var iter = Container.GetItemQueryIterator<DiscoveredStock>(query);
        while (iter.HasMoreResults)
        {
            var page = await iter.ReadNextAsync();
            return page.Resource.FirstOrDefault();
        }
        return null;
    }

    public async Task<List<DiscoveredStock>> GetAllAsync()
    {
        var results = new List<DiscoveredStock>();
        using var iter = Container.GetItemQueryIterator<DiscoveredStock>(
            new QueryDefinition("SELECT * FROM c ORDER BY c.discoveredAt DESC"));
        while (iter.HasMoreResults) { var p = await iter.ReadNextAsync(); results.AddRange(p.Resource); }
        return results;
    }

    public async Task<List<DiscoveredStock>> GetWatchlistAsync()
    {
        var query = new QueryDefinition("SELECT * FROM c WHERE c.addedToWatchlist = true");
        var results = new List<DiscoveredStock>();
        using var iter = Container.GetItemQueryIterator<DiscoveredStock>(query);
        while (iter.HasMoreResults) { var p = await iter.ReadNextAsync(); results.AddRange(p.Resource); }
        return results;
    }

    public async Task<DiscoveredStock> CreateAsync(DiscoveredStock stock)
    {
        var result = await Container.UpsertItemAsync(stock, new PartitionKey(stock.Symbol));
        return result.Resource;
    }

    public async Task<DiscoveredStock> UpdateAsync(DiscoveredStock stock)
    {
        var result = await Container.UpsertItemAsync(stock, new PartitionKey(stock.Symbol));
        return result.Resource;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        try { await Container.DeleteItemAsync<DiscoveredStock>(id, new PartitionKey(id)); return true; }
        catch { return false; }
    }
}
