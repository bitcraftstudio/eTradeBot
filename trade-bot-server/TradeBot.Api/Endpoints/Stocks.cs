using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Cosmos.Serialization.HybridRow;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net;
using System.Text.Json;
using TradeBot.Core.Interfaces;
using TradeBot.Core.Models.FMP;

namespace TradeBot.Api.Endpoints;

public class Stocks
{
    private readonly ILogger<Stocks> _logger;
    private readonly IStockService _stockService;

    public Stocks(
        IStockService stockService,
        ILogger<Stocks> logger
    ) {
        _stockService = stockService;
        _logger = logger;
    }

    [Function("Stocks-Get")]
    public async Task<HttpResponseData> Stocks_Get([HttpTrigger(AuthorizationLevel.Function, "get", Route = "stocks/{symbol}")] HttpRequestData req, string symbol)
    {
        var stock = await _stockService.GetStockAsync(symbol);
        return await HttpUtils.JsonResponse(req, stock);
    }

    [Function("Stocks-Refresh")]
    public async Task<HttpResponseData> Stocks_Enrich([HttpTrigger(AuthorizationLevel.Function, "get", Route = "stocks/refresh/{symbol}")] HttpRequestData req, string symbol)
    {
        var strEnrich = req.Query["enrich"];
        bool enrich = false;
        bool.TryParse(strEnrich, out enrich);

        var result = await _stockService.RefreshStockAsync(symbol, enrich);
        return await HttpUtils.JsonResponse(req, result);
    }
}