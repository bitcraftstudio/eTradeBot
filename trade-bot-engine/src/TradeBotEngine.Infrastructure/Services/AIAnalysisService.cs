using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TradeBotEngine.Core.Interfaces;
using TradeBotEngine.Core.Models;

namespace TradeBotEngine.Infrastructure.Services;

/// <summary>
/// AI Analysis service supporting multiple AI providers (Claude, OpenAI, xAI)
/// </summary>
public class AIAnalysisService : IAIAnalysisService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AIAnalysisService> _logger;
    private readonly IMarketDataService _marketDataService;
    private readonly INewsService _newsService;
    private readonly AIProviderConfig _config;

    public AIAnalysisService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<AIAnalysisService> logger,
        IMarketDataService marketDataService,
        INewsService newsService)
    {
        _httpClient = httpClient;
        _logger = logger;
        _marketDataService = marketDataService;
        _newsService = newsService;

        _config = new AIProviderConfig
        {
            Provider = configuration["AI:Provider"] ?? "anthropic",
            ApiKey = configuration["AI:ApiKey"] ?? "",
            Model = configuration["AI:Model"] ?? "claude-3-sonnet-20240229"
        };

        _logger.LogInformation("AI Analysis service initialized with provider: {Provider}", _config.Provider);
    }

    public async Task<MarketAnalysis> AnalyzeStockAsync(string symbol)
    {
        _logger.LogInformation("Starting complete analysis for {Symbol}", symbol);

        try
        {
            // Step 1: Get current quote
            var quote = await _marketDataService.GetQuoteAsync(symbol);
            _logger.LogInformation("Current price for {Symbol}: ${Price}", symbol, quote.Price);

            // Step 2: Get historical data and calculate indicators
            var historicalData = await _marketDataService.GetHistoricalDataAsync(symbol, 60);
            var indicators = await _marketDataService.CalculateIndicatorsAsync(symbol, historicalData);
            _logger.LogInformation("Technical indicators calculated for {Symbol}", symbol);

            // Step 3: Get technical analysis from AI
            var technicalAnalysis = await AnalyzeTechnicalsAsync(symbol, quote.Price, indicators);

            // Step 4: Get news and sentiment
            var articles = await _newsService.GetRecentNewsAsync(symbol, 7);
            var sentiment = await AnalyzeNewsSentimentAsync(symbol, articles);
            _logger.LogInformation("News sentiment for {Symbol}: {Label}", symbol, sentiment.SentimentLabel);

            // Step 5: Get trading recommendation
            var riskProfile = RiskProfile.Moderate; // Could be configurable
            var recommendation = await GetTradeRecommendationAsync(
                symbol, quote.Price, indicators, sentiment, riskProfile);

            _logger.LogInformation(
                "Analysis complete for {Symbol}: {Decision} with {Confidence}% confidence",
                symbol, recommendation.Decision, (int)(recommendation.Confidence * 100));

            return new MarketAnalysis
            {
                Symbol = symbol,
                CurrentPrice = quote.Price,
                TechnicalAnalysis = technicalAnalysis,
                Sentiment = sentiment,
                Recommendation = recommendation
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Analysis failed for {Symbol}", symbol);
            throw;
        }
    }

    public async Task<TradeRecommendation> GetTradeRecommendationAsync(
        string symbol,
        decimal currentPrice,
        TechnicalIndicators indicators,
        NewsSentimentResult sentiment,
        RiskProfile riskProfile)
    {
        var prompt = BuildTradeDecisionPrompt(symbol, currentPrice, indicators, sentiment, riskProfile);
        var response = await SendAIRequestAsync(prompt, "trading_decision");

        try
        {
            var decision = JsonSerializer.Deserialize<AITradeDecision>(response, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (decision == null)
            {
                throw new Exception("Failed to parse AI response");
            }

            return new TradeRecommendation
            {
                Symbol = symbol,
                Decision = ParseDecision(decision.Decision),
                Confidence = decision.Confidence,
                Reasoning = decision.Reasoning,
                RiskAssessment = ParseRiskAssessment(decision.RiskAssessment),
                ExpectedReturn = decision.ExpectedReturn,
                MaxRisk = decision.MaxRisk,
                TechnicalScore = (int)(decision.TechnicalScore ?? 50),
                SentimentScore = sentiment.OverallSentiment,
                MomentumScore = CalculateMomentumScore(indicators),
                SuggestedEntryPrice = decision.EntryPrice,
                SuggestedStopLoss = decision.StopLoss,
                SuggestedTakeProfit = decision.TakeProfit,
                Timestamp = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse trading decision from AI");
            return CreateDefaultRecommendation(symbol);
        }
    }

    public async Task<NewsSentimentResult> AnalyzeNewsSentimentAsync(string symbol, List<NewsArticle> articles)
    {
        if (articles.Count == 0)
        {
            // Use mock news if no real news available
            var mockArticles = _newsService.GetMockNews(symbol);
            articles = mockArticles;
        }

        var prompt = BuildSentimentPrompt(symbol, articles);
        var response = await SendAIRequestAsync(prompt, "sentiment_analysis");

        try
        {
            var sentiment = JsonSerializer.Deserialize<AISentimentResponse>(response, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (sentiment == null)
            {
                return CreateNeutralSentiment(symbol);
            }

            return new NewsSentimentResult
            {
                Symbol = symbol,
                OverallSentiment = sentiment.OverallSentiment,
                SentimentLabel = sentiment.SentimentLabel,
                Articles = articles.Select((a, i) => new AnalyzedArticle
                {
                    Title = a.Title,
                    Source = a.Source,
                    PublishedDate = a.PublishedDate,
                    Sentiment = sentiment.Articles?.ElementAtOrDefault(i)?.Sentiment ?? 0,
                    Relevance = sentiment.Articles?.ElementAtOrDefault(i)?.Relevance ?? 0.5m,
                    Summary = a.Summary
                }).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse sentiment analysis");
            return CreateNeutralSentiment(symbol);
        }
    }

    public async Task<TechnicalAnalysisResult> AnalyzeTechnicalsAsync(
        string symbol,
        decimal currentPrice,
        TechnicalIndicators indicators)
    {
        var prompt = BuildTechnicalPrompt(symbol, currentPrice, indicators);
        var response = await SendAIRequestAsync(prompt, "technical_analysis");

        try
        {
            var analysis = JsonSerializer.Deserialize<AITechnicalResponse>(response, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (analysis == null)
            {
                return CreateDefaultTechnicalAnalysis();
            }

            // Also calculate signals locally
            var signals = GenerateSignals(indicators, currentPrice);
            var (trend, strength) = DetermineTrend(indicators, currentPrice);

            return new TechnicalAnalysisResult
            {
                Trend = trend,
                Strength = strength,
                Signals = signals
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse technical analysis");
            return CreateDefaultTechnicalAnalysis();
        }
    }

    public async Task<LearningAnalysis> ExtractLearningsAsync(List<Trade> trades)
    {
        if (trades.Count == 0)
        {
            return new LearningAnalysis
            {
                KeyInsights = "No trades to analyze"
            };
        }

        var prompt = BuildLearningPrompt(trades);
        var response = await SendAIRequestAsync(prompt, "learning_extraction");

        try
        {
            var analysis = JsonSerializer.Deserialize<LearningAnalysis>(response, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return analysis ?? new LearningAnalysis { KeyInsights = "Analysis unavailable" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse learning analysis");
            return new LearningAnalysis { KeyInsights = "Analysis unavailable" };
        }
    }

    #region AI Provider Integration

    private async Task<string> SendAIRequestAsync(string prompt, string context)
    {
        return _config.Provider.ToLower() switch
        {
            "anthropic" or "claude" => await SendClaudeRequestAsync(prompt),
            "openai" or "gpt" => await SendOpenAIRequestAsync(prompt),
            "xai" or "grok" => await SendXAIRequestAsync(prompt),
            _ => throw new NotSupportedException($"AI provider {_config.Provider} not supported")
        };
    }

    private async Task<string> SendClaudeRequestAsync(string prompt)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.anthropic.com/v1/messages");
        request.Headers.Add("x-api-key", _config.ApiKey);
        request.Headers.Add("anthropic-version", "2023-06-01");

        var payload = new
        {
            model = _config.Model,
            max_tokens = 2000,
            messages = new[]
            {
                new { role = "user", content = prompt }
            }
        };

        request.Content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Claude API error: {Content}", content);
            throw new Exception($"Claude API error: {response.StatusCode}");
        }

        var result = JsonSerializer.Deserialize<ClaudeResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return result?.Content?.FirstOrDefault()?.Text ?? "";
    }

    private async Task<string> SendOpenAIRequestAsync(string prompt)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
        request.Headers.Add("Authorization", $"Bearer {_config.ApiKey}");

        var payload = new
        {
            model = _config.Model,
            messages = new[]
            {
                new { role = "system", content = "You are a professional stock trading analyst. Always respond with valid JSON only." },
                new { role = "user", content = prompt }
            },
            temperature = 0.7,
            max_tokens = 2000
        };

        request.Content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("OpenAI API error: {Content}", content);
            throw new Exception($"OpenAI API error: {response.StatusCode}");
        }

        var result = JsonSerializer.Deserialize<OpenAIResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return result?.Choices?.FirstOrDefault()?.Message?.Content ?? "";
    }

    private async Task<string> SendXAIRequestAsync(string prompt)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.x.ai/v1/chat/completions");
        request.Headers.Add("Authorization", $"Bearer {_config.ApiKey}");

        var payload = new
        {
            model = _config.Model,
            messages = new[]
            {
                new { role = "system", content = "You are a professional stock trading analyst. Always respond with valid JSON only." },
                new { role = "user", content = prompt }
            },
            temperature = 0.7,
            max_tokens = 2000
        };

        request.Content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("xAI API error: {Content}", content);
            throw new Exception($"xAI API error: {response.StatusCode}");
        }

        var result = JsonSerializer.Deserialize<OpenAIResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return result?.Choices?.FirstOrDefault()?.Message?.Content ?? "";
    }

    #endregion

    #region Prompt Building

    private string BuildTechnicalPrompt(string symbol, decimal currentPrice, TechnicalIndicators indicators)
    {
        return $@"Analyze the technical indicators for {symbol}.

Current Price: ${currentPrice}

Technical Indicators:
- RSI: {indicators.RSI:F2} (oversold < 30, overbought > 70)
- MACD: {indicators.MACD.Value:F2} (signal: {indicators.MACD.Signal:F2})
- Bollinger Bands: Upper {indicators.BollingerBands.Upper:F2}, Middle {indicators.BollingerBands.Middle:F2}, Lower {indicators.BollingerBands.Lower:F2}
- SMA 20: ${indicators.SMA20:F2}
- SMA 50: ${indicators.SMA50:F2}
- Volume: {indicators.Volume:N0} (20-day avg: {indicators.VolumeAvg20:N0})

Provide analysis in JSON format:
{{
  ""decision"": ""BUY"" | ""SELL"" | ""HOLD"",
  ""confidence"": 0.0-1.0,
  ""reasoning"": ""brief explanation"",
  ""technicalScore"": 0-100,
  ""expectedReturn"": 0.0 (as decimal),
  ""maxRisk"": 0.0 (as decimal)
}}

Respond ONLY with valid JSON.";
    }

    private string BuildSentimentPrompt(string symbol, List<NewsArticle> articles)
    {
        var articlesText = string.Join("\n\n", articles.Select((a, i) =>
            $"{i + 1}. {a.Title} ({a.Source}){(a.Summary != null ? $"\n   {a.Summary}" : "")}"));

        return $@"Analyze the sentiment of these news articles about {symbol}:

{articlesText}

Provide sentiment analysis in JSON format:
{{
  ""overallSentiment"": -1.0 to 1.0,
  ""sentimentLabel"": ""VERY_NEGATIVE"" | ""NEGATIVE"" | ""NEUTRAL"" | ""POSITIVE"" | ""VERY_POSITIVE"",
  ""analysis"": ""brief summary"",
  ""articles"": [
    {{ ""index"": 0, ""sentiment"": -1.0 to 1.0, ""relevance"": 0.0 to 1.0 }}
  ]
}}

Respond ONLY with valid JSON.";
    }

    private string BuildTradeDecisionPrompt(
        string symbol,
        decimal currentPrice,
        TechnicalIndicators indicators,
        NewsSentimentResult sentiment,
        RiskProfile riskProfile)
    {
        return $@"Make a trading decision for {symbol} (current price: ${currentPrice}).

Risk Profile: {riskProfile}

Technical Indicators:
- RSI: {indicators.RSI:F2}
- MACD: {indicators.MACD.Value:F2} (signal: {indicators.MACD.Signal:F2})
- Price vs SMA20: ${currentPrice} vs ${indicators.SMA20:F2}
- Price vs SMA50: ${currentPrice} vs ${indicators.SMA50:F2}

News Sentiment: {sentiment.SentimentLabel} ({sentiment.OverallSentiment:F2})

Based on all information and the {riskProfile} risk profile, provide a trading recommendation:
{{
  ""decision"": ""BUY"" | ""SELL"" | ""HOLD"",
  ""confidence"": 0.0-1.0,
  ""reasoning"": ""comprehensive explanation"",
  ""riskAssessment"": ""LOW"" | ""MODERATE"" | ""HIGH"" | ""VERY_HIGH"",
  ""expectedReturn"": 0.0,
  ""maxRisk"": 0.0,
  ""entryPrice"": {currentPrice},
  ""stopLoss"": 0.0,
  ""takeProfit"": 0.0,
  ""technicalScore"": 0-100,
  ""timeframe"": ""1-10 days""
}}

Respond ONLY with valid JSON.";
    }

    private string BuildLearningPrompt(List<Trade> trades)
    {
        var tradesText = string.Join("\n\n", trades.Select((t, i) =>
        {
            var outcome = t.Outcome != null
                ? $"(P/L: {t.Outcome.ProfitLossPercent:F2}%, reason: {t.Outcome.ExitReason})"
                : "(still open)";
            return $"{i + 1}. {t.Symbol} {t.Type} at ${t.EntryPrice} {outcome}\n   AI reasoning: {t.AIReasoning?.Reasoning ?? "N/A"}";
        }));

        return $@"Analyze these completed trades and extract learnings:

{tradesText}

Identify patterns and provide analysis in JSON format:
{{
  ""successfulPatterns"": [""pattern 1"", ""pattern 2""],
  ""failedPatterns"": [""pattern 1"", ""pattern 2""],
  ""recommendations"": [""adjustment 1"", ""adjustment 2""],
  ""keyInsights"": ""overall summary""
}}

Respond ONLY with valid JSON.";
    }

    #endregion

    #region Helper Methods

    private List<string> GenerateSignals(TechnicalIndicators indicators, decimal currentPrice)
    {
        var signals = new List<string>();

        if (indicators.RSI < 30)
            signals.Add("RSI oversold - potential buy opportunity");
        if (indicators.RSI > 70)
            signals.Add("RSI overbought - potential sell signal");
        if (indicators.MACD.Value > indicators.MACD.Signal)
            signals.Add("MACD bullish crossover");
        if (indicators.MACD.Value < indicators.MACD.Signal)
            signals.Add("MACD bearish crossover");
        if (currentPrice < indicators.BollingerBands.Lower * 1.02m)
            signals.Add("Near lower Bollinger Band - support level");
        if (currentPrice > indicators.BollingerBands.Upper * 0.98m)
            signals.Add("Near upper Bollinger Band - resistance level");
        if (currentPrice > indicators.SMA50)
            signals.Add("Above 50-day moving average - uptrend");
        if (currentPrice < indicators.SMA50)
            signals.Add("Below 50-day moving average - downtrend");

        return signals;
    }

    private (MarketTrend trend, int strength) DetermineTrend(TechnicalIndicators indicators, decimal currentPrice)
    {
        int bullishSignals = 0;
        int bearishSignals = 0;

        if (indicators.RSI < 30) bullishSignals += 2;
        if (indicators.RSI > 70) bearishSignals += 2;
        if (indicators.RSI > 50) bullishSignals += 1;
        if (indicators.RSI < 50) bearishSignals += 1;

        if (indicators.MACD.Value > indicators.MACD.Signal) bullishSignals += 2;
        if (indicators.MACD.Value < indicators.MACD.Signal) bearishSignals += 2;

        if (currentPrice > indicators.SMA20) bullishSignals += 1;
        if (currentPrice < indicators.SMA20) bearishSignals += 1;
        if (currentPrice > indicators.SMA50) bullishSignals += 2;
        if (currentPrice < indicators.SMA50) bearishSignals += 2;

        var totalSignals = bullishSignals + bearishSignals;
        var strength = totalSignals > 0 ? (int)(Math.Abs(bullishSignals - bearishSignals) * 100.0 / totalSignals) : 50;

        MarketTrend trend;
        if (bullishSignals > bearishSignals + 2) trend = MarketTrend.Bullish;
        else if (bearishSignals > bullishSignals + 2) trend = MarketTrend.Bearish;
        else trend = MarketTrend.Neutral;

        return (trend, strength);
    }

    private int CalculateMomentumScore(TechnicalIndicators indicators)
    {
        int score = 50;

        if (indicators.RSI > 70) score += 15;
        else if (indicators.RSI > 60) score += 10;
        else if (indicators.RSI < 30) score -= 15;
        else if (indicators.RSI < 40) score -= 10;

        if (indicators.MACD.Value > indicators.MACD.Signal)
            score += Math.Min((int)(Math.Abs(indicators.MACD.Histogram) * 5), 15);
        else
            score -= Math.Min((int)(Math.Abs(indicators.MACD.Histogram) * 5), 15);

        var volumeRatio = indicators.VolumeAvg20 > 0 ? (decimal)indicators.Volume / indicators.VolumeAvg20 : 1m;
        if (volumeRatio > 1.5m) score += 10;
        else if (volumeRatio > 1.2m) score += 5;
        else if (volumeRatio < 0.8m) score -= 5;

        if (indicators.EMA12 > indicators.EMA26) score += 10;
        else score -= 10;

        return Math.Max(0, Math.Min(100, score));
    }

    private static TradeDecision ParseDecision(string? decision) =>
        decision?.ToUpper() switch
        {
            "BUY" => TradeDecision.Buy,
            "SELL" => TradeDecision.Sell,
            _ => TradeDecision.Hold
        };

    private static RiskAssessment ParseRiskAssessment(string? assessment) =>
        assessment?.ToUpper() switch
        {
            "LOW" => RiskAssessment.Low,
            "MODERATE" => RiskAssessment.Moderate,
            "HIGH" => RiskAssessment.High,
            "VERY_HIGH" => RiskAssessment.VeryHigh,
            _ => RiskAssessment.Moderate
        };

    private TradeRecommendation CreateDefaultRecommendation(string symbol) =>
        new()
        {
            Symbol = symbol,
            Decision = TradeDecision.Hold,
            Confidence = 0.5m,
            Reasoning = "Unable to generate AI recommendation",
            RiskAssessment = RiskAssessment.Moderate
        };

    private NewsSentimentResult CreateNeutralSentiment(string symbol) =>
        new()
        {
            Symbol = symbol,
            OverallSentiment = 0,
            SentimentLabel = "NEUTRAL",
            Articles = new List<AnalyzedArticle>()
        };

    private TechnicalAnalysisResult CreateDefaultTechnicalAnalysis() =>
        new()
        {
            Trend = MarketTrend.Neutral,
            Strength = 50,
            Signals = new List<string>()
        };

    #endregion
}

#region AI Provider Config and Response Models

internal class AIProviderConfig
{
    public string Provider { get; set; } = "anthropic";
    public string ApiKey { get; set; } = "";
    public string Model { get; set; } = "";
}

internal class ClaudeResponse
{
    public List<ClaudeContent>? Content { get; set; }
}

internal class ClaudeContent
{
    public string? Text { get; set; }
}

internal class OpenAIResponse
{
    public List<OpenAIChoice>? Choices { get; set; }
}

internal class OpenAIChoice
{
    public OpenAIMessage? Message { get; set; }
}

internal class OpenAIMessage
{
    public string? Content { get; set; }
}

internal class AITradeDecision
{
    public string? Decision { get; set; }
    public decimal Confidence { get; set; }
    public string Reasoning { get; set; } = "";
    public string? RiskAssessment { get; set; }
    public decimal ExpectedReturn { get; set; }
    public decimal MaxRisk { get; set; }
    public decimal? EntryPrice { get; set; }
    public decimal? StopLoss { get; set; }
    public decimal? TakeProfit { get; set; }
    public decimal? TechnicalScore { get; set; }
}

internal class AISentimentResponse
{
    public decimal OverallSentiment { get; set; }
    public string SentimentLabel { get; set; } = "NEUTRAL";
    public List<ArticleSentiment>? Articles { get; set; }
}

internal class ArticleSentiment
{
    public int Index { get; set; }
    public decimal Sentiment { get; set; }
    public decimal Relevance { get; set; }
}

internal class AITechnicalResponse
{
    public string? Decision { get; set; }
    public decimal Confidence { get; set; }
    public string? Reasoning { get; set; }
    public int TechnicalScore { get; set; }
}

#endregion
