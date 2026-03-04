using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using TradeBot.Core.Interfaces;
using TradeBot.Core.Models.FMP;

namespace TradeBot.Api.Endpoints;

public class Collector
{
    private readonly ILogger<Collector> _logger;
    private readonly IStockService _stockService;

    public Collector(
        IStockService stockService,
        ILogger<Collector> logger
    )
    {
        _stockService = stockService;
        _logger = logger;
    }

    [Function("Stocks-Collect")]
    public async Task<HttpResponseData> CollectStocks([HttpTrigger(AuthorizationLevel.Function, "post", Route = "collect/stocks")] HttpRequestData req)
    {
        var response = req.CreateResponse();
        try
        {
            // Deserialize criteria from request body
            var criteria = await JsonSerializer.DeserializeAsync<StockScreenCriteria>(req.Body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (criteria == null)
            {
                response.StatusCode = HttpStatusCode.BadRequest;
                await response.WriteStringAsync("Invalid stock screen criteria.");
                return response;
            }

            // Call the service
            var success = await _stockService.CollectStocksAsync(criteria);
            return await HttpUtils.JsonResponse(req, success);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CollectStocks.");
            response.StatusCode = HttpStatusCode.InternalServerError;
            await response.WriteStringAsync("An error occurred.");
        }

        return response;
    }

    /*
    [Function("StockGrade-Collect")]
    public async Task<HttpResponseData> CollectStockGrade([HttpTrigger(AuthorizationLevel.Function, "get", Route = "collect/grade/{symbol}")] HttpRequestData req, string symbol)
    {
        var result = await _stockService.CollectGradeSummaryAsync(symbol);
        return await HttpUtils.JsonResponse(req, result);
    }

    [Function("IncomeStatement-Collect")]
    public async Task<HttpResponseData> CollectIncomeStatement([HttpTrigger(AuthorizationLevel.Function, "get", Route = "collect/incomestatement/{symbol}")] HttpRequestData req, string symbol)
    {
        var result = await _stockService.CollectIncomeStatementAsync(symbol);
        return await HttpUtils.JsonResponse(req, result);
    }

    [Function("BalanceSheetStatement-Collect")]
    public async Task<HttpResponseData> CollectBalanceSheetStatement([HttpTrigger(AuthorizationLevel.Function, "get", Route = "collect/balancesheetstatement/{symbol}")] HttpRequestData req, string symbol)
    {
        var result = await _stockService.CollectBalanceSheetStatementAsync(symbol);
        return await HttpUtils.JsonResponse(req, result);
    }

    [Function("CashFlowStatement-Collect")]
    public async Task<HttpResponseData> CollectCashFlowStatement([HttpTrigger(AuthorizationLevel.Function, "get", Route = "collect/cashflowstatement/{symbol}")] HttpRequestData req, string symbol)
    {
        var result = await _stockService.CollectCashFlowStatementAsync(symbol);
        return await HttpUtils.JsonResponse(req, result);
    }

    [Function("KeyMetrics-Collect")]
    public async Task<HttpResponseData> CollectKeyMetrics([HttpTrigger(AuthorizationLevel.Function, "get", Route = "collect/keymetrics/{symbol}")] HttpRequestData req, string symbol)
    {
        var result = await _stockService.CollectKeyMetricsAsync(symbol);
        return await HttpUtils.JsonResponse(req, result);
    }

    [Function("HistoricalPrices-Collect")]
    public async Task<HttpResponseData> CollectHistoricalPrices([HttpTrigger(AuthorizationLevel.Function, "get", Route = "collect/prices/{symbol}")] HttpRequestData req, string symbol)
    {
        var result = await _stockService.CollectHistoricalPricesAsync(symbol);
        return await HttpUtils.JsonResponse(req, result);
    }
    */
}