namespace TradeBotEngine.Core.Models.ETrade;

/// <summary>
/// eTrade OAuth tokens
/// </summary>
public class ETradeOAuthTokens
{
    public string AccessToken { get; set; } = string.Empty;
    public string AccessTokenSecret { get; set; } = string.Empty;
    public string RequestToken { get; set; } = string.Empty;
    public string RequestTokenSecret { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsValid => DateTime.UtcNow < ExpiresAt && !string.IsNullOrEmpty(AccessToken);
}

/// <summary>
/// eTrade account information
/// </summary>
public class ETradeAccount
{
    public string AccountId { get; set; } = string.Empty;
    public string AccountIdKey { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string AccountMode { get; set; } = string.Empty; // CASH, MARGIN
    public string AccountStatus { get; set; } = string.Empty;
    public string InstitutionType { get; set; } = string.Empty;
}

/// <summary>
/// eTrade account balance
/// </summary>
public class ETradeAccountBalance
{
    public string AccountId { get; set; } = string.Empty;
    public decimal TotalAccountValue { get; set; }
    public decimal CashBalance { get; set; }
    public decimal CashBuyingPower { get; set; }
    public decimal MarginBuyingPower { get; set; }
    public decimal SettledCash { get; set; }
    public decimal UnsettledCash { get; set; }
    public decimal NetAccountValue { get; set; }
    public decimal DayTradingBuyingPower { get; set; }
}

/// <summary>
/// eTrade position
/// </summary>
public class ETradePosition
{
    public string PositionId { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public string SecurityType { get; set; } = string.Empty; // EQ (equity)
    public int Quantity { get; set; }
    public decimal CostBasis { get; set; }
    public decimal CurrentPrice { get; set; }
    public decimal MarketValue { get; set; }
    public decimal TodaysGainLoss { get; set; }
    public decimal TotalGainLoss { get; set; }
    public decimal TotalGainLossPct { get; set; }
}

/// <summary>
/// eTrade order request
/// </summary>
public class ETradeOrderRequest
{
    public string AccountId { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public ETradeOrderAction OrderAction { get; set; }
    public ETradeOrderType OrderType { get; set; }
    public ETradeOrderDuration Duration { get; set; }
    public int Quantity { get; set; }
    public decimal? LimitPrice { get; set; }
    public decimal? StopPrice { get; set; }
    public string? ClientOrderId { get; set; }
}

public enum ETradeOrderAction
{
    Buy,
    Sell,
    BuyToCover,
    SellShort
}

public enum ETradeOrderType
{
    Market,
    Limit,
    Stop,
    StopLimit
}

public enum ETradeOrderDuration
{
    Day,
    GoodTillCancel,
    ImmediateOrCancel,
    FillOrKill
}

/// <summary>
/// eTrade order response
/// </summary>
public class ETradeOrderResponse
{
    public bool Success { get; set; }
    public long OrderId { get; set; }
    public string? OrderNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Message { get; set; }
    public decimal? FilledPrice { get; set; }
    public int? FilledQuantity { get; set; }
    public DateTime? ExecutionTime { get; set; }
}

/// <summary>
/// eTrade order status
/// </summary>
public class ETradeOrder
{
    public long OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public string OrderType { get; set; } = string.Empty;
    public string OrderAction { get; set; } = string.Empty;
    public ETradeOrderStatus Status { get; set; }
    public int OrderedQuantity { get; set; }
    public int FilledQuantity { get; set; }
    public decimal? LimitPrice { get; set; }
    public decimal? StopPrice { get; set; }
    public decimal? AvgFilledPrice { get; set; }
    public DateTime PlacedTime { get; set; }
    public DateTime? ExecutedTime { get; set; }
    public string? RejectReason { get; set; }
}

public enum ETradeOrderStatus
{
    Open,
    Executed,
    Cancelled,
    Rejected,
    Expired,
    Partial,
    Pending
}

/// <summary>
/// eTrade quote
/// </summary>
public class ETradeQuote
{
    public string Symbol { get; set; } = string.Empty;
    public decimal LastPrice { get; set; }
    public decimal Change { get; set; }
    public decimal ChangePct { get; set; }
    public decimal Bid { get; set; }
    public decimal Ask { get; set; }
    public int BidSize { get; set; }
    public int AskSize { get; set; }
    public long Volume { get; set; }
    public decimal Open { get; set; }
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public decimal PreviousClose { get; set; }
    public DateTime QuoteTime { get; set; }
}

/// <summary>
/// Configuration for eTrade API
/// </summary>
public class ETradeConfiguration
{
    public string ConsumerKey { get; set; } = string.Empty;
    public string ConsumerSecret { get; set; } = string.Empty;
    public bool UseSandbox { get; set; } = true;
    public string BaseUrl => UseSandbox 
        ? "https://apisb.etrade.com" 
        : "https://api.etrade.com";
    public string OAuthBaseUrl => UseSandbox
        ? "https://apisb.etrade.com/oauth"
        : "https://api.etrade.com/oauth";
}
