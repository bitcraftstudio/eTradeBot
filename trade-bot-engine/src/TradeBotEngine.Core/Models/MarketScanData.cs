namespace TradeBotEngine.Core.Models;

public class MarketScanResult
{
    public DateTime Timestamp { get; set; }
    public List<string> Symbols { get; set; } = new();
    public int SymbolsAnalyzed { get; set; }
    public int TradesExecuted { get; set; }
    public List<ScanRecommendation> Recommendations { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}

public class ScanRecommendation
{
    public string Symbol { get; set; } = string.Empty;
    public string Decision { get; set; } = string.Empty;
    public decimal Confidence { get; set; }
    public string Reasoning { get; set; } = string.Empty;
    public decimal Price { get; set; }
}