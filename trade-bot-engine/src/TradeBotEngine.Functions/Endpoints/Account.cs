using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using TradeBotEngine.Core.Interfaces;

namespace TradeBotEngine.Functions.Endpoints;

public class Account
{
    private readonly ILogger<Account> _logger;
    private readonly IETradeService _eTradeService;

    public Account(IETradeService eTradeService, ILogger<Account> logger)
    {
        _eTradeService = eTradeService;
        _logger = logger;
    }

    /// <summary>GET /api/accounts - Get eTrade accounts</summary>
    [Function("GetAccounts")]
    public async Task<HttpResponseData> GetAccounts([HttpTrigger(AuthorizationLevel.Function, "get", Route = "accounts")] HttpRequestData req)
    {
        try
        {
            var accounts = await _eTradeService.GetAccountsAsync();
            return await HttpUtils.JsonOk(req, accounts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get accounts");
            return await HttpUtils.JsonOther(req, HttpStatusCode.InternalServerError, ex.Message);
        }
    }

    /// <summary>GET /api/accounts/{accountId}/balance - Get account balance</summary>
    [Function("GetAccountBalance")]
    public async Task<HttpResponseData> GetAccountBalance([HttpTrigger(AuthorizationLevel.Function, "get", Route = "accounts/{accountId}/balance")] HttpRequestData req, string accountId)
    {
        try
        {
            var balance = await _eTradeService.GetAccountBalanceAsync(accountId);
            return await HttpUtils.JsonOk(req, balance);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get balance for {AccountId}", accountId);
            return await HttpUtils.JsonOther(req, HttpStatusCode.InternalServerError, ex.Message);
        }
    }

    /// <summary>GET /api/accounts/{accountId}/orders - Get orders for account</summary>
    [Function("GetOrders")]
    public async Task<HttpResponseData> GetOrders([HttpTrigger(AuthorizationLevel.Function, "get", Route = "accounts/{accountId}/orders")] HttpRequestData req, string accountId)
    {
        try
        {
            var orders = await _eTradeService.GetOrdersAsync(accountId);
            return await HttpUtils.JsonOk(req, orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get orders for {AccountId}", accountId);
            return await HttpUtils.JsonOther(req, HttpStatusCode.InternalServerError, ex.Message);
        }
    }
}