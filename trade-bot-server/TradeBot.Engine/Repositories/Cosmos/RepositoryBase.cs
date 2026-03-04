using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace TradeBot.Engine.Repositories.Cosmos
{
    public abstract class RepositoryBase
    {
        protected readonly Container Container;
        protected readonly ILogger Logger;

        protected RepositoryBase(IConfiguration config, ILogger logger, string containerName, string partitionKeyPath)
        {
            Logger = logger;
            var connectionString = config["Cosmos:ConnectionString"] ?? throw new InvalidOperationException("Cosmos:ConnectionString not configured");
            var dbName = config["Cosmos:DatabaseName"] ?? "TradeBotDB";

            var clientOptions = new CosmosClientOptions
            {
                SerializerOptions = new CosmosSerializationOptions
                {
                    PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                },
                AllowBulkExecution = true
            };

            var client = new CosmosClient(connectionString, clientOptions);
            var db = client.GetDatabase(dbName);

            // Create container if it doesn't exist (idempotent)
            Container = db.CreateContainerIfNotExistsAsync(containerName, partitionKeyPath).GetAwaiter().GetResult();
        }
    }
}
