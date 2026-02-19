using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TradeBotEngine.Core.Interfaces;
using TradeBotEngine.Infrastructure.Repositories;
using TradeBotEngine.Infrastructure.Services;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

// HTTP Clients - these register the services with typed HttpClient injection
builder.Services.AddHttpClient<IETradeService, ETradeService>();
builder.Services.AddHttpClient<IMarketDataService, MarketDataService>();
builder.Services.AddHttpClient<INewsService, NewsService>();
builder.Services.AddHttpClient<IAIAnalysisService, AIAnalysisService>();

// Infrastructure Services
builder.Services
    .AddSingleton<IRiskManagementService, RiskManagementService>()
    .AddSingleton<IStockHunterService, StockHunterService>()
    .AddSingleton<ILearningService, LearningService>()
    .AddSingleton<ITradingService, TradingService>()
    .AddSingleton<IPortfolioService, PortfolioService>()
    .AddSingleton<IPositionMonitorService, PositionMonitorService>()
    .AddSingleton<IMarketScanService, MarketScanService>();

// Repositories
builder.Services
    .AddSingleton<ITradeRepository, CosmosTradeRepository>()
    .AddSingleton<IPositionRepository, CosmosPositionRepository>()
    .AddSingleton<ILearningInsightRepository, CosmosLearningInsightRepository>()
    .AddSingleton<IMarketDataRepository, CosmosMarketDataRepository>()
    .AddSingleton<IDiscoveredStockRepository, CosmosDiscoveredStockRepository>()
    .AddSingleton<ISchedulerConfigRepository, CosmosSchedulerConfigRepository>();

builder.Build().Run();
