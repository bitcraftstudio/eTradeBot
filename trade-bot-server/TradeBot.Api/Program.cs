using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Services
    .AddHttpClient<TradeBot.Core.Interfaces.Data.IFMPService, TradeBot.Engine.Services.Data.FMPService>();

builder.Services
    .AddSingleton<TradeBot.Core.Interfaces.IStockService, TradeBot.Engine.Services.StockService>();

builder.Services
    .AddSingleton<TradeBot.Core.Interfaces.Repository.IStockRepository, TradeBot.Engine.Repositories.Cosmos.StocksRepository>();

builder.Build().Run();
