using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Web;
using TradeBot.Core.Interfaces.Data;
using TradeBot.Core.Models;
using TradeBot.Core.Models.FMP;

namespace TradeBot.Engine.Services.Data;

/// <summary>
/// Financial Modeling Prep API integration service
/// </summary>
public class FMPService : IFMPService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<FMPService> _logger;

    private readonly string? _apiKey;
    private readonly string? _apiUrl;

    public FMPService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<FMPService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;

        _apiKey = _configuration["FMP:ApiKey"];
        _apiUrl = _configuration["FMP:ApiUrl"];

        // Set base address for FMP API
        if(!String.IsNullOrEmpty(_apiUrl))
        {
            _httpClient.BaseAddress = new Uri(_apiUrl);
        }   
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    public async Task<bool> IsApiConfiguredAsync()
    {
        return !string.IsNullOrEmpty(_apiKey) && !string.IsNullOrEmpty(_apiUrl);
    }


    public async Task<List<StockScreenResult>> ScreenStocksAsync(StockScreenCriteria criteria)
    {
        if (!await IsApiConfiguredAsync())
        {
            _logger.LogWarning("FMP API key or URL not configured");
            return new List<StockScreenResult>();
        }

        try
        {
            var queryParams = BuildScreeningQueryParams(criteria);
            var response = await _httpClient.GetAsync(AppendApiKey($"company-screener?{queryParams}"));

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("FMP API returned {StatusCode} for stock screening", response.StatusCode);
                return new List<StockScreenResult>();
            }

            var data = await response.Content.ReadFromJsonAsync<List<StockScreenResult>>();
            return data ?? new List<StockScreenResult>();
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Error calling FMP stock screening API");
            return new List<StockScreenResult>();
        }
    }


    public async Task<CompanyProfile?> GetCompanyProfileAsync(string symbol)
    {
        if (!await IsApiConfiguredAsync())
        {
            _logger.LogWarning("FMP API key or URL not configured");
            return null;
        }

        try
        {
            var response = await _httpClient.GetAsync(AppendApiKey($"profile?symbol={symbol}"));

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("FMP API returned {StatusCode} for company profile {Symbol}", response.StatusCode, symbol);
                return null;
            }

            var data = await response.Content.ReadFromJsonAsync<List<CompanyProfile>>();
            return data.FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling FMP company profile API for {Symbol}", symbol);
            return null;
        }
    }

    public async Task<StockQuote?> GetStockQuoteAsync(string symbol)
    {
        if (!await IsApiConfiguredAsync())
        {
            _logger.LogWarning("FMP API key or URL not configured");
            return null;
        }

        try
        {
            var response = await _httpClient.GetAsync(AppendApiKey($"quote?symbol={symbol}"));

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("FMP API returned {StatusCode} for stock quote {Symbol}", response.StatusCode, symbol);
                return null;
            }
            var data = await response.Content.ReadFromJsonAsync<List<StockQuote>>();
            return data.FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling FMP stock quote API for {Symbol}", symbol);
            return null;
        }
    }


    public async Task<List<HistoricalGrade>> GetHistoricalGradesAsync(string symbol, int limit = 100)
    {
        if (!await IsApiConfiguredAsync())
        {
            _logger.LogWarning("FMP API key or URL not configured");
            return new List<HistoricalGrade>();
        }

        try
        {
            var response = await _httpClient.GetAsync(AppendApiKey($"grades-historical?symbol={symbol}"));

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("FMP API returned {StatusCode} for historial grades {Symbol}", response.StatusCode, symbol);
                return new List<HistoricalGrade>();
            }

            var data = await response.Content.ReadFromJsonAsync<List<HistoricalGrade>>();
            return data != null ? data.Where(x => x.Date > DateTime.Now.AddYears(-1)).ToList() : new List<Core.Models.FMP.HistoricalGrade>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling FMP stock quote API for {Symbol}", symbol);
            return new List<HistoricalGrade>();
        }
    }


    public async Task<List<StockGrade>> GetStockGradesAsync(string symbol)
    {
        if (!await IsApiConfiguredAsync())
        {
            _logger.LogWarning("FMP API key or URL not configured");
            return new List<StockGrade>();
        }

        try
        {
            var response = await _httpClient.GetAsync(AppendApiKey($"grades?symbol={symbol}"));

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("FMP API returned {StatusCode} for grades {Symbol}", response.StatusCode, symbol);
                return new List<StockGrade>();
            }

            var data = await response.Content.ReadFromJsonAsync<List<Core.Models.FMP.StockGrade>>();
            return data != null ? data : new List<StockGrade>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling FMP stock quote API for {Symbol}", symbol);
            return new List<StockGrade>();
        }
    }


    public async Task<Core.Models.FMP.StockGradeSummary?> GetStockGradeSummaryAsync(string symbol)
    {
        if (!await IsApiConfiguredAsync())
        {
            _logger.LogWarning("FMP API key or URL not configured");
            return null;
        }

        try
        {
            var response = await _httpClient.GetAsync(AppendApiKey($"grades-consensus?symbol={symbol}"));

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("FMP API returned {StatusCode} for grades {Symbol}", response.StatusCode, symbol);
                return null;
            }

            var data = await response.Content.ReadFromJsonAsync<List<Core.Models.FMP.StockGradeSummary>>();
            return data != null ? data.FirstOrDefault() : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling FMP stock quote API for {Symbol}", symbol);
            return null;
        }
    }

    public async Task<List<StockCandleStick>> GetHistoricalPricesAsync(string symbol, int days)
    {
        if (!await IsApiConfiguredAsync())
        {
            _logger.LogWarning("FMP API key or URL not configured");
            return new List<StockCandleStick>();
        }

        try
        {
            var queryParams = BuildDateRangeQueryParams(symbol, days);
            var response = await _httpClient.GetAsync(AppendApiKey($"historical-price-eod/full?{queryParams}"));

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("FMP API returned {StatusCode} for grades {Symbol}", response.StatusCode, symbol);
                return new List<StockCandleStick>();
            }

            var data = await response.Content.ReadFromJsonAsync<List<StockCandleStick>>();
            return data != null ? data : new List<StockCandleStick>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling FMP stock quote API for {Symbol}", symbol);
            return new List<StockCandleStick>();
        }
    }

    public async Task<IncomeStatement?> GetIncomeStatementAsync(string symbol)
    {
        if (!await IsApiConfiguredAsync())
        {
            _logger.LogWarning("FMP API key or URL not configured");
            return null;
        }

        try
        {
            var response = await _httpClient.GetAsync(AppendApiKey($"income-statement?symbol={symbol}"));
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("FMP API returned {StatusCode} for income statement {Symbol}", response.StatusCode, symbol);
                return null;
            }

            var data = await response.Content.ReadFromJsonAsync<List<IncomeStatement>>();
            return data != null ? data.FirstOrDefault() : null;
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Error calling FMP income statement API for {Symbol}", symbol);
            return null;
        }
    }

    public async Task<BalanceSheet?> GetBalanceSheetStatementAsync(string symbol)
    {
        if (!await IsApiConfiguredAsync())
        {
            _logger.LogWarning("FMP API key or URL not configured");
            return null;
        }

        try
        {
            var response = await _httpClient.GetAsync(AppendApiKey($"balance-sheet-statement?symbol={symbol}"));
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("FMP API returned {StatusCode} for balance sheet statement {Symbol}", response.StatusCode, symbol);
                return null;
            }

            var data = await response.Content.ReadFromJsonAsync<List<BalanceSheet>>();
            return data != null ? data.FirstOrDefault() : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling FMP balance sheet API for {Symbol}", symbol);
            return null;
        }
    }

    public async Task<CashFlowStatement?> GetCashFlowStatementAsync(string symbol)
    {
        if (!await IsApiConfiguredAsync())
        {
            _logger.LogWarning("FMP API key or URL not configured");
            return null;
        }

        try
        {
            var response = await _httpClient.GetAsync(AppendApiKey($"cash-flow-statement?symbol={symbol}"));
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("FMP API returned {StatusCode} for cash flow statement {Symbol}", response.StatusCode, symbol);
                return null;
            }

            var data = await response.Content.ReadFromJsonAsync<List<CashFlowStatement>>();
            return data != null ? data.FirstOrDefault() : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling FMP cash flow statement API for {Symbol}", symbol);
            return null;
        }
    }

    public async Task<KeyMetrics?> GetKeyMetricsAsync(string symbol)
    {
        if (!await IsApiConfiguredAsync())
        {
            _logger.LogWarning("FMP API key or URL not configured");
            return null;
        }

        try
        {
            var response = await _httpClient.GetAsync(AppendApiKey($"key-metrics?symbol={symbol}"));
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("FMP API returned {StatusCode} for key metrics {Symbol}", response.StatusCode, symbol);
                return null;
            }

            var data = await response.Content.ReadFromJsonAsync<List<KeyMetrics>>();
            return data != null ? data.FirstOrDefault() : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling FMP key metrics API for {Symbol}", symbol);
            return null;
        }
    }

    public async Task<RatingsSnapshot?> GetRatingsSnapshotAsync(string symbol)
    {
        if (!await IsApiConfiguredAsync())
        {
            _logger.LogWarning("FMP API key or URL not configured");
            return null;
        }

        try
        {
            var response = await _httpClient.GetAsync(AppendApiKey($"ratings-snapshot?symbol={symbol}"));
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("FMP API returned {StatusCode} for Ratings Snapshot {Symbol}", response.StatusCode, symbol);
                return null;
            }

            var data = await response.Content.ReadFromJsonAsync<List<RatingsSnapshot>>();
            return data != null ? data.FirstOrDefault() : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling FMP Ratings Snapshot API for {Symbol}", symbol);
            return null;
        }
    }

    public async Task<PriceTargetConsensus?> GetPriceTargetConsensusAsync(string symbol)
    {
        if (!await IsApiConfiguredAsync())
        {
            _logger.LogWarning("FMP API key or URL not configured");
            return null;
        }

        try
        {
            var response = await _httpClient.GetAsync(AppendApiKey($"price-target-consensus?symbol={symbol}"));
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("FMP API returned {StatusCode} for Price Target Consensus {Symbol}", response.StatusCode, symbol);
                return null;
            }

            var data = await response.Content.ReadFromJsonAsync<List<PriceTargetConsensus>>();
            return data != null ? data.FirstOrDefault() : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling FMP Price Target Consensus API for {Symbol}", symbol);
            return null;
        }
    }


    private string AppendApiKey(string path)
    {
        if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(_apiKey))
        {
            return path;
        }
        var separator = path.Contains('?') ? '&' : '?';
        return $"{path}{separator}apikey={_apiKey}";
    }

    private string BuildDateRangeQueryParams(string symbol, int days)
    {
        var startDate = DateTime.UtcNow.AddDays(-days).ToString("yyyy-MM-dd");
        var endDate = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var parameters = new List<string>();

        parameters.Add($"from={startDate}");
        parameters.Add($"to={endDate}"); 
        parameters.Add($"symbol={symbol}");

        return string.Join("&", parameters);
    }

    private string BuildScreeningQueryParams(Core.Models.FMP.StockScreenCriteria criteria)
    {
        var parameters = new List<string>();

        if (criteria.MarketCapMoreThan.HasValue)
            parameters.Add($"marketCapMoreThan={criteria.MarketCapMoreThan.Value}");
        if (criteria.MarketCapLowerThan.HasValue)
            parameters.Add($"marketCapLowerThan={criteria.MarketCapLowerThan.Value}");
        if (!string.IsNullOrEmpty(criteria.Sector))
            parameters.Add($"sector={HttpUtility.UrlEncode(criteria.Sector)}");
        if (!string.IsNullOrEmpty(criteria.Industry))
            parameters.Add($"industry={HttpUtility.UrlEncode(criteria.Industry)}");
        if (criteria.BetaMoreThan.HasValue)
            parameters.Add($"betaMoreThan={criteria.BetaMoreThan.Value}");
        if (criteria.BetaLessThan.HasValue)
            parameters.Add($"betaLowerThan={criteria.BetaLessThan.Value}");
        if (criteria.PriceMoreThan.HasValue)
            parameters.Add($"priceMoreThan={criteria.PriceMoreThan.Value}");
        if (criteria.PriceLowerThan.HasValue)
            parameters.Add($"priceLowerThan={criteria.PriceLowerThan.Value}");
        if (criteria.DividendMoreThan.HasValue)
            parameters.Add($"priceMoreThan={criteria.DividendMoreThan.Value}");
        if (criteria.DividendLowerThan.HasValue)
            parameters.Add($"priceLowerThan={criteria.DividendLowerThan.Value}");
        if (criteria.VolumeMoreThan.HasValue)
            parameters.Add($"priceMoreThan={criteria.VolumeMoreThan.Value}");
        if (criteria.VolumeLowerThan.HasValue)
            parameters.Add($"priceLowerThan={criteria.VolumeLowerThan.Value}");
        if (!string.IsNullOrEmpty(criteria.Exchange))
            parameters.Add($"exchange={HttpUtility.UrlEncode(criteria.Exchange)}");
        if (!string.IsNullOrEmpty(criteria.Country))
            parameters.Add($"country={HttpUtility.UrlEncode(criteria.Country)}");
        if (criteria.IsEtf.HasValue)
            parameters.Add($"isEtf={criteria.IsEtf.Value.ToString().ToLower()}");
        if (criteria.IsFund.HasValue)
            parameters.Add($"isFund={criteria.IsFund.Value.ToString().ToLower()}");
        if (criteria.IsActivelyTrading.HasValue)
            parameters.Add($"isActivelyTrading={criteria.IsActivelyTrading.Value.ToString().ToLower()}");
        if (criteria.Limit.HasValue)
            parameters.Add($"limit={criteria.Limit.Value}");

        return string.Join("&", parameters);
    }


}