using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TradeBotEngine.Core.Interfaces;
using TradeBotEngine.Core.Models;

namespace TradeBotEngine.Infrastructure.Services;

/// <summary>
/// Portfolio service - manages positions and syncs with eTrade account
/// </summary>
public class PortfolioService : IPortfolioService
{
    private readonly IETradeService _eTradeService;
    private readonly IMarketDataService _marketDataService;
    private readonly IPositionRepository _positionRepo;
    private readonly IConfiguration _config;
    private readonly ILogger<PortfolioService> _logger;

    public PortfolioService(
        IETradeService eTradeService,
        IMarketDataService marketDataService,
        IPositionRepository positionRepo,
        IConfiguration config,
        ILogger<PortfolioService> logger)
    {
        _eTradeService = eTradeService;
        _marketDataService = marketDataService;
        _positionRepo = positionRepo;
        _config = config;
        _logger = logger;
    }

    public async Task<PortfolioSummary> GetPortfolioSummaryAsync()
    {
        var openPositions = await _positionRepo.GetByStatusAsync(PositionStatus.Open);
        var initialCapital = decimal.Parse(_config["Trading:InitialCapital"] ?? "10000");

        decimal investedValue = 0;
        decimal currentValue = 0;
        decimal cashBalance = initialCapital;

        var positionDetails = new List<PositionSummary>();

        // Try to get live balance from eTrade
        var accountId = _config["ETrade:DefaultAccountId"] ?? "";
        if (!string.IsNullOrEmpty(accountId))
        {
            try
            {
                var balance = await _eTradeService.GetAccountBalanceAsync(accountId);
                cashBalance = balance.CashBalance;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not fetch eTrade balance, using estimated cash");
            }
        }

        foreach (var position in openPositions)
        {
            try
            {
                var quote = await _marketDataService.GetQuoteAsync(position.Symbol);
                var posValue = quote.Price * position.Quantity;
                var costBasis = position.EntryPrice * position.Quantity;
                var pnl = posValue - costBasis;
                var pnlPct = costBasis > 0 ? (pnl / costBasis) * 100 : 0;

                investedValue += costBasis;
                currentValue += posValue;

                positionDetails.Add(new PositionSummary
                {
                    Symbol = position.Symbol,
                    Quantity = position.Quantity,
                    AvgPrice = position.EntryPrice,
                    CurrentPrice = quote.Price,
                    CurrentValue = posValue,
                    UnrealizedPnL = pnl,
                    UnrealizedPnLPercent = pnlPct
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not get quote for {Symbol}", position.Symbol);
            }
        }

        var totalValue = cashBalance + currentValue;
        var totalPnL = totalValue - initialCapital;
        var totalPnLPct = initialCapital > 0 ? (totalPnL / initialCapital) * 100 : 0;

        return new PortfolioSummary
        {
            TotalValue = totalValue,
            CashBalance = cashBalance,
            InvestedValue = investedValue,
            TotalPnL = totalPnL,
            TotalPnLPercent = totalPnLPct,
            OpenPositions = openPositions.Count,
            Positions = positionDetails,
            LastUpdated = DateTime.UtcNow
        };
    }

    public async Task<List<Position>> GetOpenPositionsAsync() =>
        await _positionRepo.GetByStatusAsync(PositionStatus.Open);

    public async Task<Position?> GetPositionAsync(string symbol) =>
        await _positionRepo.GetBySymbolAsync(symbol);

    public async Task<Position> CreatePositionAsync(Position position) =>
        await _positionRepo.CreateAsync(position);

    public async Task<Position> UpdatePositionAsync(Position position) =>
        await _positionRepo.UpdateAsync(position);

    public async Task<bool> ClosePositionAsync(string symbol)
    {
        var position = await _positionRepo.GetBySymbolAsync(symbol);
        if (position == null) return false;
        position.Status = PositionStatus.Closed;
        await _positionRepo.UpdateAsync(position);
        return true;
    }

    public async Task SyncWithETradeAsync()
    {
        var accountId = _config["ETrade:DefaultAccountId"] ?? "";
        if (string.IsNullOrEmpty(accountId))
        {
            _logger.LogWarning("No ETrade:DefaultAccountId configured - skipping sync");
            return;
        }

        _logger.LogInformation("Syncing portfolio with eTrade account {AccountId}", accountId);

        var eTradePositions = await _eTradeService.GetPositionsAsync(accountId);
        var localPositions = await _positionRepo.GetByStatusAsync(PositionStatus.Open);

        foreach (var etp in eTradePositions)
        {
            var local = localPositions.FirstOrDefault(p => p.Symbol == etp.Symbol);
            if (local != null)
            {
                local.CurrentPrice = etp.CurrentPrice;
                local.ETradePositionId = etp.PositionId;
                local.ETradeAccountId = accountId;
                local.UnrealizedPnL = etp.TotalGainLoss;
                await _positionRepo.UpdateAsync(local);
            }
        }

        // Close positions that no longer exist in eTrade
        var eTradeSymbols = eTradePositions.Select(p => p.Symbol).ToHashSet();
        foreach (var local in localPositions.Where(p => !eTradeSymbols.Contains(p.Symbol)))
        {
            local.Status = PositionStatus.Closed;
            await _positionRepo.UpdateAsync(local);
            _logger.LogInformation("Closed orphaned position for {Symbol} (not found in eTrade)", local.Symbol);
        }

        _logger.LogInformation("Portfolio sync complete: {Count} positions synced", eTradePositions.Count);
    }
}
