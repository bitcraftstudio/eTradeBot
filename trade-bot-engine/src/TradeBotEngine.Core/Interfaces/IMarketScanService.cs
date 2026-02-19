using TradeBotEngine.Core.Models;

namespace TradeBotEngine.Core.Interfaces;

public interface IMarketScanService
{
    Task<MarketScanResult> PerformMarketScanAsync();
}