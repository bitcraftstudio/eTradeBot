using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TradeBotEngine.Core.Interfaces;
using TradeBotEngine.Core.Models;

namespace TradeBotEngine.Infrastructure.Services;

/// <summary>
/// News service - fetches news articles via Alpha Vantage / NewsAPI and provides mock data for testing
/// </summary>
public class NewsService : INewsService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly ILogger<NewsService> _logger;

    public NewsService(HttpClient httpClient, IConfiguration config, ILogger<NewsService> logger)
    {
        _httpClient = httpClient;
        _config = config;
        _logger = logger;
    }

    public async Task<List<NewsArticle>> GetRecentNewsAsync(string symbol, int days = 7)
    {
        // Try Alpha Vantage News Sentiment API
        var apiKey = _config["News:AlphaVantageApiKey"];
        if (!string.IsNullOrEmpty(apiKey) && apiKey != "YOUR_ALPHA_VANTAGE_KEY")
        {
            try
            {
                return await FetchAlphaVantageNewsAsync(symbol, apiKey, days);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Alpha Vantage news fetch failed for {Symbol}", symbol);
            }
        }

        // Fall back to mock news
        _logger.LogInformation("Using mock news for {Symbol}", symbol);
        return GetMockNews(symbol);
    }

    public async Task<NewsArticle> SaveNewsArticleAsync(NewsArticle article)
    {
        // In this implementation, news is fetched on-demand and not persisted.
        // Could be extended to store to Cosmos DB for caching.
        return await Task.FromResult(article);
    }

    public List<NewsArticle> GetMockNews(string symbol)
    {
        var baseDate = DateTime.UtcNow;
        return new List<NewsArticle>
        {
            new()
            {
                Symbol = symbol,
                Title = $"{symbol} Reports Strong Q4 Earnings, Beats Estimates",
                Source = "Reuters",
                PublishedDate = baseDate.AddDays(-1),
                Summary = $"{symbol} reported quarterly earnings that exceeded analyst expectations, driven by strong revenue growth and margin expansion.",
                Sentiment = 0.7m,
                Relevance = 0.95m
            },
            new()
            {
                Symbol = symbol,
                Title = $"Analysts Raise Price Target for {symbol} Following Product Launch",
                Source = "Bloomberg",
                PublishedDate = baseDate.AddDays(-2),
                Summary = $"Multiple analysts have raised their price targets for {symbol} following the announcement of a new flagship product.",
                Sentiment = 0.6m,
                Relevance = 0.90m
            },
            new()
            {
                Symbol = symbol,
                Title = $"{symbol} Navigates Market Headwinds with Cost Reduction Strategy",
                Source = "MarketWatch",
                PublishedDate = baseDate.AddDays(-3),
                Summary = $"{symbol} implements cost-saving measures amid global economic uncertainty, maintaining profitability targets.",
                Sentiment = 0.2m,
                Relevance = 0.80m
            },
            new()
            {
                Symbol = symbol,
                Title = $"Institutional Investors Increase Holdings in {symbol}",
                Source = "CNBC",
                PublishedDate = baseDate.AddDays(-4),
                Summary = $"13F filings show significant institutional accumulation of {symbol} shares in the latest quarter.",
                Sentiment = 0.5m,
                Relevance = 0.75m
            },
            new()
            {
                Symbol = symbol,
                Title = $"{symbol} Expands into Emerging Markets, Eyes Long-Term Growth",
                Source = "Wall Street Journal",
                PublishedDate = baseDate.AddDays(-5),
                Summary = $"{symbol} announces expansion plans targeting emerging markets in Asia and Latin America.",
                Sentiment = 0.4m,
                Relevance = 0.70m
            }
        };
    }

    private async Task<List<NewsArticle>> FetchAlphaVantageNewsAsync(string symbol, string apiKey, int days)
    {
        var url = $"https://www.alphavantage.co/query?function=NEWS_SENTIMENT&tickers={symbol}&apikey={apiKey}&limit=50";
        var response = await _httpClient.GetStringAsync(url);

        using var doc = System.Text.Json.JsonDocument.Parse(response);
        var root = doc.RootElement;

        if (!root.TryGetProperty("feed", out var feed))
            return new List<NewsArticle>();

        var cutoff = DateTime.UtcNow.AddDays(-days);
        var articles = new List<NewsArticle>();

        foreach (var item in feed.EnumerateArray())
        {
            var publishedStr = item.GetProperty("time_published").GetString() ?? "";
            if (!DateTime.TryParseExact(publishedStr, "yyyyMMddTHHmmss",
                null, System.Globalization.DateTimeStyles.AssumeUniversal, out var published))
                continue;

            if (published < cutoff) continue;

            var overallSentiment = 0m;
            if (item.TryGetProperty("overall_sentiment_score", out var sentEl))
                overallSentiment = sentEl.GetDecimal();

            articles.Add(new NewsArticle
            {
                Symbol = symbol,
                Title = item.GetProperty("title").GetString() ?? "",
                Source = item.TryGetProperty("source", out var src) ? src.GetString() ?? "" : "",
                PublishedDate = published,
                Url = item.TryGetProperty("url", out var urlEl) ? urlEl.GetString() : null,
                Summary = item.TryGetProperty("summary", out var sum) ? sum.GetString() : null,
                Sentiment = overallSentiment,
                Relevance = 0.8m
            });
        }

        _logger.LogInformation("Fetched {Count} news articles for {Symbol} from Alpha Vantage", articles.Count, symbol);
        return articles;
    }
}
