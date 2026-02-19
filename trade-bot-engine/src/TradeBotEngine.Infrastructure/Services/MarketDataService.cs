using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using TradeBotEngine.Core.Interfaces;
using TradeBotEngine.Core.Models;

namespace TradeBotEngine.Infrastructure.Services;

/// <summary>
/// Market data service using Alpha Vantage API (primary) with Yahoo Finance fallback
/// </summary>
public class MarketDataService : IMarketDataService
{
    private readonly ILogger<MarketDataService> _logger;
    private readonly IMarketDataRepository _marketDataRepository;
    private readonly HttpClient _httpClient;
    private readonly string _alphaVantageApiKey;
    private readonly string _alphaVantageBaseUrl = "https://www.alphavantage.co/query";

    public MarketDataService(
        ILogger<MarketDataService> logger,
        IMarketDataRepository marketDataRepository,
        HttpClient httpClient,
        IConfiguration configuration)
    {
        _logger = logger;
        _marketDataRepository = marketDataRepository;
        _httpClient = httpClient;
        _alphaVantageApiKey = configuration["News:AlphaVantageApiKey"] ?? "";

        if (string.IsNullOrEmpty(_alphaVantageApiKey) || _alphaVantageApiKey == "YOUR_ALPHA_VANTAGE_KEY")
        {
            _logger.LogWarning("Alpha Vantage API key not configured. Get a free key at https://www.alphavantage.co/support/#api-key");
        }
    }

    public async Task<StockQuote> GetQuoteAsync(string symbol)
    {
        try
        {
            _logger.LogDebug("Fetching quote for {Symbol}", symbol);

            // Try Alpha Vantage first
            if (!string.IsNullOrEmpty(_alphaVantageApiKey) && _alphaVantageApiKey != "YOUR_ALPHA_VANTAGE_KEY")
            {
                try
                {
                    return await GetQuoteFromAlphaVantageAsync(symbol);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Alpha Vantage failed for {Symbol}, trying fallback", symbol);
                }
            }

            // Fallback to Yahoo Finance with better error handling
            return await GetQuoteFromYahooAsync(symbol);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch quote for {Symbol}", symbol);
            throw;
        }
    }

    private async Task<StockQuote> GetQuoteFromAlphaVantageAsync(string symbol)
    {
        var url = $"{_alphaVantageBaseUrl}?function=GLOBAL_QUOTE&symbol={symbol}&apikey={_alphaVantageApiKey}";
        
        var response = await _httpClient.GetStringAsync(url);
        var json = JsonDocument.Parse(response);

        // Check for API limit message
        if (json.RootElement.TryGetProperty("Note", out var note))
        {
            throw new Exception($"Alpha Vantage API limit reached: {note.GetString()}");
        }

        if (json.RootElement.TryGetProperty("Error Message", out var error))
        {
            throw new Exception($"Alpha Vantage error: {error.GetString()}");
        }

        var globalQuote = json.RootElement.GetProperty("Global Quote");

        var price = decimal.Parse(globalQuote.GetProperty("05. price").GetString() ?? "0");
        var previousClose = decimal.Parse(globalQuote.GetProperty("08. previous close").GetString() ?? "0");
        var change = decimal.Parse(globalQuote.GetProperty("09. change").GetString() ?? "0");
        var changePercent = decimal.Parse(globalQuote.GetProperty("10. change percent").GetString()?.TrimEnd('%') ?? "0");
        var open = decimal.Parse(globalQuote.GetProperty("02. open").GetString() ?? "0");
        var high = decimal.Parse(globalQuote.GetProperty("03. high").GetString() ?? "0");
        var low = decimal.Parse(globalQuote.GetProperty("04. low").GetString() ?? "0");
        var volume = long.Parse(globalQuote.GetProperty("06. volume").GetString() ?? "0");

        return new StockQuote
        {
            Symbol = symbol,
            Price = price,
            PreviousClose = previousClose,
            Change = change,
            ChangePercent = changePercent,
            Open = open,
            High = high,
            Low = low,
            Volume = volume,
            MarketCap = null, // Alpha Vantage quote doesn't include market cap
            Timestamp = DateTime.UtcNow
        };
    }

    private async Task<StockQuote> GetQuoteFromYahooAsync(string symbol)
    {
        try
        {
            // Use Yahoo Finance v8 API (more reliable than v7)
            var url = $"https://query1.finance.yahoo.com/v8/finance/chart/{symbol}?interval=1d&range=1d";
            
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
            request.Headers.Add("Accept", "application/json");
            
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(content);
            
            var result = json.RootElement.GetProperty("chart").GetProperty("result")[0];
            var meta = result.GetProperty("meta");
            var quote = result.GetProperty("indicators").GetProperty("quote")[0];
            
            var regularMarketPrice = meta.GetProperty("regularMarketPrice").GetDecimal();
            var previousClose = meta.GetProperty("chartPreviousClose").GetDecimal();
            
            // Get today's OHLCV
            var opens = quote.GetProperty("open");
            var highs = quote.GetProperty("high");
            var lows = quote.GetProperty("low");
            var volumes = quote.GetProperty("volume");
            
            var lastIndex = opens.GetArrayLength() - 1;
            
            return new StockQuote
            {
                Symbol = symbol,
                Price = regularMarketPrice,
                PreviousClose = previousClose,
                Change = regularMarketPrice - previousClose,
                ChangePercent = previousClose != 0 ? ((regularMarketPrice - previousClose) / previousClose) * 100 : 0,
                Open = opens[lastIndex].ValueKind != JsonValueKind.Null ? opens[lastIndex].GetDecimal() : regularMarketPrice,
                High = highs[lastIndex].ValueKind != JsonValueKind.Null ? highs[lastIndex].GetDecimal() : regularMarketPrice,
                Low = lows[lastIndex].ValueKind != JsonValueKind.Null ? lows[lastIndex].GetDecimal() : regularMarketPrice,
                Volume = volumes[lastIndex].ValueKind != JsonValueKind.Null ? volumes[lastIndex].GetInt64() : 0,
                MarketCap = null,
                Timestamp = DateTime.UtcNow
            };
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            _logger.LogError("Yahoo Finance returned 401 Unauthorized. The API may be blocking requests.");
            throw new Exception($"Unable to fetch quote for {symbol}. Yahoo Finance API is blocking requests. Please configure Alpha Vantage API key.", ex);
        }
    }

    public async Task<List<StockQuote>> GetQuotesAsync(IEnumerable<string> symbols)
    {
        var quotes = new List<StockQuote>();
        foreach (var symbol in symbols)
        {
            try
            {
                var quote = await GetQuoteAsync(symbol);
                quotes.Add(quote);
                
                // Rate limiting - Alpha Vantage free tier is 5 calls/minute
                if (!string.IsNullOrEmpty(_alphaVantageApiKey) && _alphaVantageApiKey != "YOUR_ALPHA_VANTAGE_KEY")
                {
                    await Task.Delay(250); // 4 per second max
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch quote for {Symbol}", symbol);
            }
        }
        return quotes;
    }

    public async Task<List<CandleData>> GetHistoricalDataAsync(string symbol, int days)
    {
        try
        {
            _logger.LogDebug("Fetching {Days} days of historical data for {Symbol}", days, symbol);

            // Try Alpha Vantage first
            if (!string.IsNullOrEmpty(_alphaVantageApiKey) && _alphaVantageApiKey != "YOUR_ALPHA_VANTAGE_KEY")
            {
                try
                {
                    return await GetHistoricalFromAlphaVantageAsync(symbol, days);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Alpha Vantage historical data failed for {Symbol}, trying fallback", symbol);
                }
            }

            // Fallback to Yahoo Finance
            return await GetHistoricalFromYahooAsync(symbol, days);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch historical data for {Symbol}", symbol);
            throw;
        }
    }

    private async Task<List<CandleData>> GetHistoricalFromAlphaVantageAsync(string symbol, int days)
    {
        var url = $"{_alphaVantageBaseUrl}?function=TIME_SERIES_DAILY&symbol={symbol}&outputsize={(days > 100 ? "full" : "compact")}&apikey={_alphaVantageApiKey}";
        
        var response = await _httpClient.GetStringAsync(url);
        var json = JsonDocument.Parse(response);

        if (json.RootElement.TryGetProperty("Note", out var note))
        {
            throw new Exception($"Alpha Vantage API limit: {note.GetString()}");
        }

        var timeSeries = json.RootElement.GetProperty("Time Series (Daily)");
        var candles = new List<CandleData>();

        foreach (var day in timeSeries.EnumerateObject().Take(days))
        {
            candles.Add(new CandleData
            {
                Date = DateTime.Parse(day.Name),
                Open = decimal.Parse(day.Value.GetProperty("1. open").GetString() ?? "0"),
                High = decimal.Parse(day.Value.GetProperty("2. high").GetString() ?? "0"),
                Low = decimal.Parse(day.Value.GetProperty("3. low").GetString() ?? "0"),
                Close = decimal.Parse(day.Value.GetProperty("4. close").GetString() ?? "0"),
                Volume = long.Parse(day.Value.GetProperty("5. volume").GetString() ?? "0")
            });
        }

        // Return in chronological order
        candles.Reverse();
        return candles;
    }

    private async Task<List<CandleData>> GetHistoricalFromYahooAsync(string symbol, int days)
    {
        var range = days switch
        {
            <= 5 => "5d",
            <= 30 => "1mo",
            <= 90 => "3mo",
            <= 180 => "6mo",
            <= 365 => "1y",
            _ => "2y"
        };

        var url = $"https://query1.finance.yahoo.com/v8/finance/chart/{symbol}?interval=1d&range={range}";
        
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
        request.Headers.Add("Accept", "application/json");
        
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        
        var result = json.RootElement.GetProperty("chart").GetProperty("result")[0];
        var timestamps = result.GetProperty("timestamp");
        var quote = result.GetProperty("indicators").GetProperty("quote")[0];
        
        var opens = quote.GetProperty("open");
        var highs = quote.GetProperty("high");
        var lows = quote.GetProperty("low");
        var closes = quote.GetProperty("close");
        var volumes = quote.GetProperty("volume");
        
        var candles = new List<CandleData>();
        
        for (int i = 0; i < timestamps.GetArrayLength(); i++)
        {
            if (closes[i].ValueKind == JsonValueKind.Null) continue;
            
            candles.Add(new CandleData
            {
                Date = DateTimeOffset.FromUnixTimeSeconds(timestamps[i].GetInt64()).DateTime,
                Open = opens[i].ValueKind != JsonValueKind.Null ? opens[i].GetDecimal() : closes[i].GetDecimal(),
                High = highs[i].ValueKind != JsonValueKind.Null ? highs[i].GetDecimal() : closes[i].GetDecimal(),
                Low = lows[i].ValueKind != JsonValueKind.Null ? lows[i].GetDecimal() : closes[i].GetDecimal(),
                Close = closes[i].GetDecimal(),
                Volume = volumes[i].ValueKind != JsonValueKind.Null ? volumes[i].GetInt64() : 0
            });
        }

        return candles.TakeLast(days).ToList();
    }

    public Task<TechnicalIndicators> CalculateIndicatorsAsync(string symbol, List<CandleData> candles)
    {
        if (candles.Count < 50)
        {
            _logger.LogWarning("Insufficient data for {Symbol}: {Count} candles", symbol, candles.Count);
        }

        var prices = candles.Select(c => c.Close).ToList();
        var volumes = candles.Select(c => c.Volume).ToList();

        var indicators = new TechnicalIndicators
        {
            RSI = CalculateRSI(prices, 14),
            MACD = CalculateMACD(prices),
            BollingerBands = CalculateBollingerBands(prices, 20),
            SMA20 = CalculateSMA(prices, 20),
            SMA50 = CalculateSMA(prices, 50),
            EMA12 = CalculateEMA(prices, 12),
            EMA26 = CalculateEMA(prices, 26),
            Volume = volumes.LastOrDefault(),
            VolumeAvg20 = volumes.Count >= 20 
                ? (long)volumes.TakeLast(20).Average() 
                : (long)volumes.DefaultIfEmpty(0).Average()
        };

        _logger.LogDebug("Calculated indicators for {Symbol}: RSI={RSI:F2}, MACD={MACD:F2}",
            symbol, indicators.RSI, indicators.MACD.Value);

        return Task.FromResult(indicators);
    }

    public async Task CacheMarketDataAsync(string symbol, List<CandleData> candles)
    {
        foreach (var candle in candles)
        {
            var marketData = new MarketData
            {
                Symbol = symbol,
                Date = candle.Date,
                Open = candle.Open,
                High = candle.High,
                Low = candle.Low,
                Close = candle.Close,
                Volume = candle.Volume
            };

            await _marketDataRepository.UpsertAsync(marketData);
        }

        _logger.LogInformation("Cached {Count} candles for {Symbol}", candles.Count, symbol);
    }

    public async Task<List<MarketData>> GetCachedDataAsync(string symbol, int days)
    {
        return await _marketDataRepository.GetBySymbolAsync(symbol, days);
    }

    #region Technical Indicator Calculations

    private static decimal CalculateRSI(List<decimal> prices, int period)
    {
        if (prices.Count < period + 1)
            return 50m; // Neutral RSI if not enough data

        var gains = new List<decimal>();
        var losses = new List<decimal>();

        for (int i = 1; i < prices.Count; i++)
        {
            var change = prices[i] - prices[i - 1];
            gains.Add(change > 0 ? change : 0);
            losses.Add(change < 0 ? Math.Abs(change) : 0);
        }

        var avgGain = gains.TakeLast(period).Average();
        var avgLoss = losses.TakeLast(period).Average();

        if (avgLoss == 0)
            return 100m;

        var rs = avgGain / avgLoss;
        return 100m - (100m / (1m + rs));
    }

    private static MACDData CalculateMACD(List<decimal> prices)
    {
        var ema12 = CalculateEMA(prices, 12);
        var ema26 = CalculateEMA(prices, 26);
        var macdLine = ema12 - ema26;

        // Signal line is 9-period EMA of MACD line
        // Simplified: use the current MACD as approximate signal
        var signal = macdLine * 0.9m; // Approximation

        return new MACDData
        {
            Value = macdLine,
            Signal = signal,
            Histogram = macdLine - signal
        };
    }

    private static BollingerBandsData CalculateBollingerBands(List<decimal> prices, int period)
    {
        var sma = CalculateSMA(prices, period);
        var recentPrices = prices.TakeLast(period).ToList();

        if (recentPrices.Count < period)
        {
            return new BollingerBandsData { Upper = sma, Middle = sma, Lower = sma };
        }

        var squaredDiffs = recentPrices.Select(p => (p - sma) * (p - sma)).ToList();
        var variance = squaredDiffs.Average();
        var stdDev = (decimal)Math.Sqrt((double)variance);

        return new BollingerBandsData
        {
            Upper = sma + (2 * stdDev),
            Middle = sma,
            Lower = sma - (2 * stdDev)
        };
    }

    private static decimal CalculateSMA(List<decimal> prices, int period)
    {
        if (prices.Count < period)
            return prices.DefaultIfEmpty(0).Average();

        return prices.TakeLast(period).Average();
    }

    private static decimal CalculateEMA(List<decimal> prices, int period)
    {
        if (prices.Count == 0)
            return 0m;

        if (prices.Count < period)
            return prices.Average();

        var multiplier = 2m / (period + 1);
        var ema = prices.Take(period).Average(); // Start with SMA

        foreach (var price in prices.Skip(period))
        {
            ema = (price - ema) * multiplier + ema;
        }

        return ema;
    }

    #endregion
}
