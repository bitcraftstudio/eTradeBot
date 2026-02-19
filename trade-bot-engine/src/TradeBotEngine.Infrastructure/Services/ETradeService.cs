using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Web;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TradeBotEngine.Core.Interfaces;
using TradeBotEngine.Core.Models.ETrade;

namespace TradeBotEngine.Infrastructure.Services;

/// <summary>
/// eTrade API service implementation using OAuth 1.0a
/// </summary>
public class ETradeService : IETradeService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ETradeService> _logger;
    private readonly ETradeConfiguration _config;
    private ETradeOAuthTokens _tokens = new();

    private const string OAuthVersion = "1.0";
    private const string OAuthSignatureMethod = "HMAC-SHA1";

    public ETradeService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<ETradeService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _config = new ETradeConfiguration
        {
            ConsumerKey = configuration["ETrade:ConsumerKey"] ?? "",
            ConsumerSecret = configuration["ETrade:ConsumerSecret"] ?? "",
            UseSandbox = bool.Parse(configuration["ETrade:UseSandbox"] ?? "true")
        };

        _logger.LogInformation("eTrade service initialized. Sandbox: {UseSandbox}", _config.UseSandbox);
    }

    #region Authentication

    public async Task<string> GetAuthorizationUrlAsync()
    {
        _logger.LogInformation("Starting OAuth flow - getting request token");

        var requestTokenUrl = $"{_config.OAuthBaseUrl}/request_token";
        var callbackUrl = "oob"; // Out-of-band for desktop apps

        var oauthParams = new Dictionary<string, string>
        {
            ["oauth_consumer_key"] = _config.ConsumerKey,
            ["oauth_signature_method"] = OAuthSignatureMethod,
            ["oauth_timestamp"] = GetTimestamp(),
            ["oauth_nonce"] = GetNonce(),
            ["oauth_version"] = OAuthVersion,
            ["oauth_callback"] = callbackUrl
        };

        var signature = GenerateSignature("GET", requestTokenUrl, oauthParams, "", "");
        oauthParams["oauth_signature"] = signature;

        var request = new HttpRequestMessage(HttpMethod.Get, requestTokenUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("OAuth", BuildOAuthHeader(oauthParams));

        var response = await _httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to get request token: {Content}", content);
            throw new Exception($"Failed to get request token: {content}");
        }

        // Parse request token response
        var tokenParams = HttpUtility.ParseQueryString(content);
        _tokens.RequestToken = tokenParams["oauth_token"] ?? "";
        _tokens.RequestTokenSecret = tokenParams["oauth_token_secret"] ?? "";

        var authorizeUrl = $"https://us.etrade.com/e/t/etws/authorize?key={_config.ConsumerKey}&token={_tokens.RequestToken}";
        
        _logger.LogInformation("Authorization URL generated. User must authorize at: {Url}", authorizeUrl);
        
        return authorizeUrl;
    }

    public async Task<ETradeOAuthTokens> CompleteAuthorizationAsync(string verifier)
    {
        _logger.LogInformation("Completing OAuth flow with verifier");

        var accessTokenUrl = $"{_config.OAuthBaseUrl}/access_token";

        var oauthParams = new Dictionary<string, string>
        {
            ["oauth_consumer_key"] = _config.ConsumerKey,
            ["oauth_token"] = _tokens.RequestToken,
            ["oauth_signature_method"] = OAuthSignatureMethod,
            ["oauth_timestamp"] = GetTimestamp(),
            ["oauth_nonce"] = GetNonce(),
            ["oauth_version"] = OAuthVersion,
            ["oauth_verifier"] = verifier
        };

        var signature = GenerateSignature("GET", accessTokenUrl, oauthParams, _tokens.RequestTokenSecret, "");
        oauthParams["oauth_signature"] = signature;

        var request = new HttpRequestMessage(HttpMethod.Get, accessTokenUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("OAuth", BuildOAuthHeader(oauthParams));

        var response = await _httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to get access token: {Content}", content);
            throw new Exception($"Failed to get access token: {content}");
        }

        // Parse access token response
        var tokenParams = HttpUtility.ParseQueryString(content);
        _tokens.AccessToken = tokenParams["oauth_token"] ?? "";
        _tokens.AccessTokenSecret = tokenParams["oauth_token_secret"] ?? "";
        _tokens.ExpiresAt = DateTime.UtcNow.AddDays(1); // eTrade tokens expire at midnight ET

        _logger.LogInformation("Successfully obtained access tokens");
        
        return _tokens;
    }

    public async Task<bool> RefreshTokensAsync()
    {
        if (string.IsNullOrEmpty(_tokens.AccessToken))
        {
            _logger.LogWarning("No access token to refresh");
            return false;
        }

        _logger.LogInformation("Refreshing access token");

        var renewUrl = $"{_config.OAuthBaseUrl}/renew_access_token";

        var oauthParams = new Dictionary<string, string>
        {
            ["oauth_consumer_key"] = _config.ConsumerKey,
            ["oauth_token"] = _tokens.AccessToken,
            ["oauth_signature_method"] = OAuthSignatureMethod,
            ["oauth_timestamp"] = GetTimestamp(),
            ["oauth_nonce"] = GetNonce(),
            ["oauth_version"] = OAuthVersion
        };

        var signature = GenerateSignature("GET", renewUrl, oauthParams, _tokens.AccessTokenSecret, "");
        oauthParams["oauth_signature"] = signature;

        var request = new HttpRequestMessage(HttpMethod.Get, renewUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("OAuth", BuildOAuthHeader(oauthParams));

        var response = await _httpClient.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            _tokens.ExpiresAt = DateTime.UtcNow.AddHours(2);
            _logger.LogInformation("Successfully refreshed access token");
            return true;
        }

        _logger.LogWarning("Failed to refresh token");
        return false;
    }

    public Task<bool> IsAuthenticatedAsync()
    {
        return Task.FromResult(_tokens.IsValid);
    }

    #endregion

    #region Accounts

    public async Task<List<ETradeAccount>> GetAccountsAsync()
    {
        var url = $"{_config.BaseUrl}/v1/accounts/list.json";
        var response = await SendAuthenticatedRequestAsync<AccountListResponse>(HttpMethod.Get, url);

        return response?.AccountList?.Accounts?.Select(a => new ETradeAccount
        {
            AccountId = a.AccountId,
            AccountIdKey = a.AccountIdKey,
            AccountType = a.AccountType,
            AccountName = a.AccountName ?? "",
            AccountMode = a.AccountMode ?? "",
            AccountStatus = a.AccountStatus ?? ""
        }).ToList() ?? new List<ETradeAccount>();
    }

    public async Task<ETradeAccountBalance> GetAccountBalanceAsync(string accountIdKey)
    {
        var url = $"{_config.BaseUrl}/v1/accounts/{accountIdKey}/balance.json?instType=BROKERAGE&realTimeNAV=true";
        var response = await SendAuthenticatedRequestAsync<BalanceResponse>(HttpMethod.Get, url);

        var computed = response?.BalanceData?.Computed;
        var cash = response?.BalanceData?.Cash;

        return new ETradeAccountBalance
        {
            AccountId = accountIdKey,
            TotalAccountValue = computed?.RealTimeValues?.TotalAccountValue ?? 0,
            CashBalance = computed?.CashBalance ?? 0,
            CashBuyingPower = computed?.CashBuyingPower ?? 0,
            MarginBuyingPower = computed?.MarginBuyingPower ?? 0,
            SettledCash = cash?.FundsForOpenOrdersCash ?? 0,
            NetAccountValue = computed?.RealTimeValues?.NetAccountValue ?? 0,
            DayTradingBuyingPower = computed?.DayTradingBuyingPower ?? 0
        };
    }

    public async Task<List<ETradePosition>> GetPositionsAsync(string accountIdKey)
    {
        var url = $"{_config.BaseUrl}/v1/accounts/{accountIdKey}/portfolio.json";
        var response = await SendAuthenticatedRequestAsync<PortfolioResponse>(HttpMethod.Get, url);

        var positions = new List<ETradePosition>();
        var accountPortfolio = response?.PortfolioData?.AccountPortfolio;

        if (accountPortfolio?.Position != null)
        {
            foreach (var pos in accountPortfolio.Position)
            {
                positions.Add(new ETradePosition
                {
                    PositionId = pos.PositionId?.ToString() ?? "",
                    Symbol = pos.Product?.Symbol ?? "",
                    SecurityType = pos.Product?.SecurityType ?? "EQ",
                    Quantity = (int)(pos.Quantity ?? 0),
                    CostBasis = pos.CostPerShare ?? 0,
                    CurrentPrice = pos.Quick?.LastTrade ?? 0,
                    MarketValue = pos.MarketValue ?? 0,
                    TodaysGainLoss = pos.TodaysGainLoss ?? 0,
                    TotalGainLoss = pos.TotalGain ?? 0,
                    TotalGainLossPct = pos.TotalGainPct ?? 0
                });
            }
        }

        return positions;
    }

    #endregion

    #region Orders

    public async Task<ETradeOrderResponse> PlaceOrderAsync(ETradeOrderRequest request)
    {
        _logger.LogInformation("Placing order: {Action} {Quantity} {Symbol}", request.OrderAction, request.Quantity, request.Symbol);

        // First, preview the order
        var previewResponse = await PreviewOrderAsync(request);
        if (!previewResponse.Success)
        {
            return previewResponse;
        }

        var url = $"{_config.BaseUrl}/v1/accounts/{request.AccountId}/orders/place.json";

        var orderPayload = BuildOrderPayload(request, previewResponse.OrderId);

        var response = await SendAuthenticatedRequestAsync<PlaceOrderResponse>(
            HttpMethod.Post, 
            url, 
            JsonSerializer.Serialize(orderPayload));

        if (response?.OrderResponse?.OrderId != null)
        {
            return new ETradeOrderResponse
            {
                Success = true,
                OrderId = response.OrderResponse.OrderId.Value,
                Status = "PLACED",
                Message = "Order placed successfully"
            };
        }

        return new ETradeOrderResponse
        {
            Success = false,
            Message = "Failed to place order"
        };
    }

    public async Task<ETradeOrderResponse> PreviewOrderAsync(ETradeOrderRequest request)
    {
        var url = $"{_config.BaseUrl}/v1/accounts/{request.AccountId}/orders/preview.json";

        var orderPayload = BuildOrderPayload(request);

        var response = await SendAuthenticatedRequestAsync<PreviewOrderResponse>(
            HttpMethod.Post,
            url,
            JsonSerializer.Serialize(orderPayload));

        if (response?.Preview?.PreviewId != null)
        {
            return new ETradeOrderResponse
            {
                Success = true,
                OrderId = response.Preview.PreviewId.Value,
                Message = "Order preview successful"
            };
        }

        return new ETradeOrderResponse
        {
            Success = false,
            Message = response?.Preview?.Message?.FirstOrDefault()?.Description ?? "Preview failed"
        };
    }

    public async Task<bool> CancelOrderAsync(string accountIdKey, long orderId)
    {
        var url = $"{_config.BaseUrl}/v1/accounts/{accountIdKey}/orders/cancel.json";

        var payload = new { CancelOrderRequest = new { orderId } };
        var response = await SendAuthenticatedRequestAsync<CancelOrderResponse>(
            HttpMethod.Put,
            url,
            JsonSerializer.Serialize(payload));

        return response?.CancelData?.OrderId != null;
    }

    public async Task<List<ETradeOrder>> GetOrdersAsync(string accountIdKey)
    {
        var url = $"{_config.BaseUrl}/v1/accounts/{accountIdKey}/orders.json";
        var response = await SendAuthenticatedRequestAsync<OrdersResponse>(HttpMethod.Get, url);

        var orders = new List<ETradeOrder>();
        if (response?.Orders?.Order != null)
        {
            foreach (var order in response.Orders.Order)
            {
                orders.Add(MapToETradeOrder(order));
            }
        }

        return orders;
    }

    public async Task<ETradeOrder?> GetOrderAsync(string accountIdKey, long orderId)
    {
        var orders = await GetOrdersAsync(accountIdKey);
        return orders.FirstOrDefault(o => o.OrderId == orderId);
    }

    #endregion

    #region Market Data

    public async Task<ETradeQuote> GetQuoteAsync(string symbol)
    {
        var quotes = await GetQuotesAsync(new[] { symbol });
        return quotes.FirstOrDefault() ?? new ETradeQuote { Symbol = symbol };
    }

    public async Task<List<ETradeQuote>> GetQuotesAsync(IEnumerable<string> symbols)
    {
        var symbolList = string.Join(",", symbols);
        var url = $"{_config.BaseUrl}/v1/market/quote/{symbolList}.json";
        
        var response = await SendAuthenticatedRequestAsync<QuoteResponse>(HttpMethod.Get, url);

        var quotes = new List<ETradeQuote>();
        if (response?.Quote?.QuoteItems != null)
        {
            foreach (var q in response.Quote.QuoteItems)
            {
                var all = q.All;
                quotes.Add(new ETradeQuote
                {
                    Symbol = q.Product?.Symbol ?? "",
                    LastPrice = all?.LastTrade ?? 0,
                    Change = all?.ChangeClose ?? 0,
                    ChangePct = all?.ChangeClosePercentage ?? 0,
                    Bid = all?.Bid ?? 0,
                    Ask = all?.Ask ?? 0,
                    BidSize = all?.BidSize ?? 0,
                    AskSize = all?.AskSize ?? 0,
                    Volume = all?.TotalVolume ?? 0,
                    Open = all?.Open ?? 0,
                    High = all?.High ?? 0,
                    Low = all?.Low ?? 0,
                    PreviousClose = all?.PreviousClose ?? 0,
                    QuoteTime = DateTime.UtcNow
                });
            }
        }

        return quotes;
    }

    #endregion

    #region Helper Methods

    private async Task<T?> SendAuthenticatedRequestAsync<T>(HttpMethod method, string url, string? body = null)
    {
        if (!_tokens.IsValid)
        {
            throw new InvalidOperationException("Not authenticated. Please complete OAuth flow first.");
        }

        var oauthParams = new Dictionary<string, string>
        {
            ["oauth_consumer_key"] = _config.ConsumerKey,
            ["oauth_token"] = _tokens.AccessToken,
            ["oauth_signature_method"] = OAuthSignatureMethod,
            ["oauth_timestamp"] = GetTimestamp(),
            ["oauth_nonce"] = GetNonce(),
            ["oauth_version"] = OAuthVersion
        };

        var signature = GenerateSignature(method.Method, url.Split('?')[0], oauthParams, _tokens.AccessTokenSecret, "");
        oauthParams["oauth_signature"] = signature;

        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("OAuth", BuildOAuthHeader(oauthParams));
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        if (!string.IsNullOrEmpty(body))
        {
            request.Content = new StringContent(body, Encoding.UTF8, "application/json");
        }

        var response = await _httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("API request failed: {StatusCode} - {Content}", response.StatusCode, content);
            throw new HttpRequestException($"API request failed: {response.StatusCode}");
        }

        return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true 
        });
    }

    private string GenerateSignature(string method, string url, Dictionary<string, string> oauthParams, string tokenSecret, string additionalParams)
    {
        var sortedParams = oauthParams.OrderBy(p => p.Key).ToDictionary(p => p.Key, p => p.Value);
        var paramString = string.Join("&", sortedParams.Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));

        if (!string.IsNullOrEmpty(additionalParams))
        {
            paramString += "&" + additionalParams;
        }

        var signatureBase = $"{method.ToUpper()}&{Uri.EscapeDataString(url)}&{Uri.EscapeDataString(paramString)}";
        var signingKey = $"{Uri.EscapeDataString(_config.ConsumerSecret)}&{Uri.EscapeDataString(tokenSecret)}";

        using var hmac = new HMACSHA1(Encoding.UTF8.GetBytes(signingKey));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(signatureBase));
        return Convert.ToBase64String(hash);
    }

    private static string BuildOAuthHeader(Dictionary<string, string> oauthParams)
    {
        return string.Join(", ", oauthParams.Select(p => $"{p.Key}=\"{Uri.EscapeDataString(p.Value)}\""));
    }

    private static string GetTimestamp() => DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

    private static string GetNonce() => Guid.NewGuid().ToString("N");

    private static object BuildOrderPayload(ETradeOrderRequest request, long? previewId = null)
    {
        var orderDetail = new
        {
            orderTerm = request.Duration.ToString().ToUpper(),
            marketSession = "REGULAR",
            priceType = request.OrderType.ToString().ToUpper(),
            limitPrice = request.LimitPrice,
            stopPrice = request.StopPrice,
            Instrument = new[]
            {
                new
                {
                    Product = new
                    {
                        symbol = request.Symbol,
                        securityType = "EQ"
                    },
                    orderAction = request.OrderAction.ToString().ToUpper(),
                    quantityType = "QUANTITY",
                    quantity = request.Quantity
                }
            }
        };

        if (previewId.HasValue)
        {
            return new
            {
                PlaceOrderRequest = new
                {
                    orderType = "EQ",
                    clientOrderId = request.ClientOrderId ?? Guid.NewGuid().ToString("N")[..20],
                    PreviewIds = new[] { new { previewId = previewId.Value } },
                    Order = new[] { orderDetail }
                }
            };
        }

        return new
        {
            PreviewOrderRequest = new
            {
                orderType = "EQ",
                clientOrderId = request.ClientOrderId ?? Guid.NewGuid().ToString("N")[..20],
                Order = new[] { orderDetail }
            }
        };
    }

    private static ETradeOrder MapToETradeOrder(dynamic order)
    {
        // Convert dynamic value to string to aid overload resolution and avoid compile-time ambiguity.
        var orderStatusValue = Convert.ToString(order.orderStatus) ?? string.Empty;

        // Explicitly declare the out variable type to avoid 'out var' inference issues.
        ETradeOrderStatus status;
        var parsed = Enum.TryParse<ETradeOrderStatus>(orderStatusValue, true, out status);

        return new ETradeOrder
        {
            OrderId = order.orderId ?? 0,
            OrderNumber = order.orderNumber ?? "",
            Symbol = order.OrderDetail?[0]?.Instrument?[0]?.Product?.symbol ?? "",
            OrderType = order.OrderDetail?[0]?.priceType ?? "",
            OrderAction = order.OrderDetail?[0]?.Instrument?[0]?.orderAction ?? "",
            Status = parsed ? status : ETradeOrderStatus.Pending,
            OrderedQuantity = (int)(order.OrderDetail?[0]?.Instrument?[0]?.quantity ?? 0),
            FilledQuantity = (int)(order.OrderDetail?[0]?.Instrument?[0]?.filledQuantity ?? 0),
            LimitPrice = order.OrderDetail?[0]?.limitPrice,
            StopPrice = order.OrderDetail?[0]?.stopPrice,
            PlacedTime = DateTime.UtcNow
        };
    }

    #endregion
}

#region API Response Models

internal class AccountListResponse
{
    public AccountListData? AccountList { get; set; }
}

internal class AccountListData
{
    public List<AccountData>? Accounts { get; set; }
}

internal class AccountData
{
    public string AccountId { get; set; } = "";
    public string AccountIdKey { get; set; } = "";
    public string AccountType { get; set; } = "";
    public string? AccountName { get; set; }
    public string? AccountMode { get; set; }
    public string? AccountStatus { get; set; }
}

internal class BalanceResponse
{
    public BalanceData? BalanceData { get; set; }
}

internal class BalanceData
{
    public ComputedBalance? Computed { get; set; }
    public CashBalance? Cash { get; set; }
}

internal class ComputedBalance
{
    public decimal CashBalance { get; set; }
    public decimal CashBuyingPower { get; set; }
    public decimal MarginBuyingPower { get; set; }
    public decimal DayTradingBuyingPower { get; set; }
    public RealTimeValues? RealTimeValues { get; set; }
}

internal class RealTimeValues
{
    public decimal TotalAccountValue { get; set; }
    public decimal NetAccountValue { get; set; }
}

internal class CashBalance
{
    public decimal FundsForOpenOrdersCash { get; set; }
}

internal class PortfolioResponse
{
    public PortfolioData? PortfolioData { get; set; }
}

internal class PortfolioData
{
    public AccountPortfolioData? AccountPortfolio { get; set; }
}

internal class AccountPortfolioData
{
    public List<PositionData>? Position { get; set; }
}

internal class PositionData
{
    public long? PositionId { get; set; }
    public ProductData? Product { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? CostPerShare { get; set; }
    public decimal? MarketValue { get; set; }
    public decimal? TodaysGainLoss { get; set; }
    public decimal? TotalGain { get; set; }
    public decimal? TotalGainPct { get; set; }
    public QuickData? Quick { get; set; }
}

internal class ProductData
{
    public string? Symbol { get; set; }
    public string? SecurityType { get; set; }
}

internal class QuickData
{
    public decimal? LastTrade { get; set; }
}

internal class PreviewOrderResponse
{
    public PreviewData? Preview { get; set; }
}

internal class PreviewData
{
    public long? PreviewId { get; set; }
    public List<MessageData>? Message { get; set; }
}

internal class MessageData
{
    public string? Description { get; set; }
}

internal class PlaceOrderResponse
{
    public OrderResponseData? OrderResponse { get; set; }
}

internal class OrderResponseData
{
    public long? OrderId { get; set; }
}

internal class CancelOrderResponse
{
    public CancelData? CancelData { get; set; }
}

internal class CancelData
{
    public long? OrderId { get; set; }
}

internal class OrdersResponse
{
    public OrdersData? Orders { get; set; }
}

internal class OrdersData
{
    public List<dynamic>? Order { get; set; }
}

internal class QuoteResponse
{
    public QuoteData? Quote { get; set; }
}

internal class QuoteData
{
    public List<QuoteDataItem>? QuoteItems { get; set; }
}

internal class QuoteDataItem
{
    public ProductData? Product { get; set; }
    public AllQuoteData? All { get; set; }
}

internal class AllQuoteData
{
    public decimal? LastTrade { get; set; }
    public decimal? ChangeClose { get; set; }
    public decimal? ChangeClosePercentage { get; set; }
    public decimal? Bid { get; set; }
    public decimal? Ask { get; set; }
    public int? BidSize { get; set; }
    public int? AskSize { get; set; }
    public long? TotalVolume { get; set; }
    public decimal? Open { get; set; }
    public decimal? High { get; set; }
    public decimal? Low { get; set; }
    public decimal? PreviousClose { get; set; }
}

#endregion
