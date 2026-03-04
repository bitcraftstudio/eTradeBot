using Microsoft.Extensions.Logging;
using System.Diagnostics;
using TradeBot.Core.Interfaces;
using TradeBot.Core.Interfaces.Data;
using TradeBot.Core.Interfaces.Repository;
using TradeBot.Core.Models;
using TradeBot.Core.Models.FMP;

namespace TradeBot.Engine.Services
{
    public class StockService : IStockService
    {
        private readonly IStockRepository _stockRepository;
        private readonly IFMPService _fmpService;
        private readonly ILogger<StockService> _logger;

        public StockService(
            IStockRepository stocksRepository,
            IFMPService fmpService,
            ILogger<StockService> logger
        ) {
            _stockRepository = stocksRepository;
            _fmpService = fmpService;
            _logger = logger;
        }

        public async Task<Stock?> GetStockAsync(string symbol)
        {
            try
            {
                var response = await _stockRepository.GetBySymbolAsync(symbol);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Error GetGradeSummaryAsync");
                return null;
            }
        }

        public async Task<bool> CollectStocksAsync(StockScreenCriteria criteria)
        {
            try
            {
                var stocks = await _fmpService.ScreenStocksAsync(criteria);
                var symbols = stocks.Select(s => s.Symbol).ToList();
                var existingSymbols = await _stockRepository.GetExistingSymbolsAsync(symbols);
                var existingSet = new HashSet<string>(existingSymbols);

                var newStocks = stocks.Where(s => !existingSet.Contains(s.Symbol))
                    .Select(stock => new Stock
                    {
                        Id = stock.Symbol,
                        Symbol = stock.Symbol,
                        Name = stock.CompanyName,
                        Sector = stock.Sector,
                        IsEtf = stock.IsEtf.HasValue ? stock.IsEtf.Value : false,
                        Exchange = stock.Exchange,
                        Dividend = stock.LastAnnualDividend.HasValue ? stock.LastAnnualDividend.Value : 0,
                        Volume = stock.Volume,
                        MarketCap = stock.MarketCap,
                        Price = stock.Price,
                    })
                    .ToList();

                await _stockRepository.BulkUpsertAsync(newStocks);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Error CollectStockDataAsync");
                return false;
            }
        }

        public async Task<bool> RefreshStockAsync(string symbol, bool enrich = true)
        {
            var stock = await GetStockAsync(symbol);
            if (stock == null) {  return false; }

            var profile = await _fmpService.GetCompanyProfileAsync(symbol);
            if (profile == null) { return false; }

            stock.Dividend = profile.LastDividend;
            stock.Volume = profile.Volume;
            stock.MarketCap = profile.MarketCap;
            stock.Price = profile.Price;

            if (enrich) { 
                var taskCashflowStatement = _fmpService.GetCashFlowStatementAsync(symbol);
                var taskIncomeStatement = _fmpService.GetIncomeStatementAsync(symbol);
                var taskBalanceSheet = _fmpService.GetBalanceSheetStatementAsync(symbol);
                var taskKeyMetrics = _fmpService.GetKeyMetricsAsync(symbol);
                var taskGradeSummary = _fmpService.GetStockGradeSummaryAsync(symbol);
                var taskHistoricalPrices = _fmpService.GetHistoricalPricesAsync(symbol, 100);
                var taskHistoricalGrades = _fmpService.GetHistoricalGradesAsync(symbol);
                var ratingsSnapshotTask = _fmpService.GetRatingsSnapshotAsync(symbol);
                var priceTargetTask = _fmpService.GetPriceTargetConsensusAsync(symbol);

                await Task.WhenAll(taskCashflowStatement, taskIncomeStatement, taskBalanceSheet, taskKeyMetrics, taskGradeSummary, taskHistoricalPrices, ratingsSnapshotTask, priceTargetTask);

                var keyMetrics = taskKeyMetrics.Result;
                var gradeSummary = taskGradeSummary.Result;
                var prices = taskHistoricalPrices.Result;
                var incomeStatement = taskIncomeStatement.Result;
                var cashFlow = taskCashflowStatement.Result;
                var ratingsSnapshot = ratingsSnapshotTask.Result;
                var priceTarget = priceTargetTask.Result;

                stock.CashFlowStatement = taskCashflowStatement.Result;
                stock.IncomeStatement = taskIncomeStatement.Result;
                stock.BalanceSheet = taskBalanceSheet.Result;
                stock.KeyMetrics = taskKeyMetrics.Result;
                stock.GradeSummary = taskGradeSummary.Result;
                stock.CandleSticks = taskHistoricalPrices.Result;

                if(stock.GradeSummary != null && stock.GradeSummary.HistoricalEntries != null)
                {
                    stock.GradeSummary.HistoricalEntries = taskHistoricalGrades.Result;
                }

                double grossProfit = incomeStatement.GrossProfit.HasValue ? (double)incomeStatement.GrossProfit.Value : 0.0;
                double revenue = incomeStatement.Revenue.HasValue ? (double)incomeStatement.Revenue.Value : 0.0;
                double operatingIncome = incomeStatement.OperatingIncome.HasValue ? (double)incomeStatement.OperatingIncome.Value : 0.0;
                double freeCashFlow = cashFlow.FreeCashFlow.HasValue ? (double)cashFlow.FreeCashFlow.Value : 0.0;
                double roe = keyMetrics.ReturnOnEquity.HasValue ? (double)keyMetrics.ReturnOnEquity.Value : 0.0;
                double roePct = roe * 100;
                double evToEbitda = keyMetrics.EvToEBITDA.HasValue ? (double)keyMetrics.EvToEBITDA.Value : 0.0;

                double grossMargin = revenue != 0 ? (grossProfit / revenue) * 100 : 0.0;
                double opMargin = revenue != 0 ? (operatingIncome / revenue) * 100 : 0.0;
                double fcfMargin = revenue != 0 ? (freeCashFlow / revenue) * 100 : 0.0;

                // Calculate fundamental score: Weights profitability and margins
                double fund = Math.Clamp((roePct * 0.4 + grossMargin * 0.2 + opMargin * 0.2 + fcfMargin * 0.2) / 10, 0, 10);
                double val = Math.Clamp(10 - evToEbitda * 0.15, 0, 10);

                var closes = prices.OrderByDescending(p => p.Date).Select(p => (double)p.Close).ToArray();
                double sma20 = (double)closes.Take(20).Average();
                double sma50 = (double)closes.Take(50).Average();
                double rsi14 = CalculateRSI(closes, 14);
                double vol = CalculateAnnualVol(closes);

                double tech = 5.0 + (closes[0] > sma20 ? 1.5 : -1.0) + (sma20 > sma50 ? 1.5 : -1.0) + (rsi14 < 40 ? 2.0 : rsi14 > 70 ? -2.0 : 0.0) + (vol < 25 ? 1.0 : -1.0);

                if (closes.Length < 20) { tech = 5.0; /* neutral */ }  // Minimal for SMA20
                if (closes.Length < 50) { /*warn or adjust */ }
                if (evToEbitda <= 0) { val = 5.0; /* neutral */ }

                double analystScore = (gradeSummary.Consensus == "Buy") ? 8.0 : 5.0;
                double momentumTweak = (closes[0] / sma20 - 1.0) * 2.0;

                double smartScore = Math.Round((fund + val + tech + analystScore) / 4.0 + momentumTweak, 1);
                smartScore = Math.Clamp(smartScore, 0, 10);

                stock.SmartScore = (int)Math.Round(smartScore, 0);
                stock.Consensus = gradeSummary.Consensus;
                stock.ConsensusTrending = CalculateConsensusTrend(gradeSummary.HistoricalEntries.ToList());
                stock.Rating = ratingsSnapshot != null ? ratingsSnapshot.Rating : string.Empty;
                stock.PriceTarget = priceTarget != null ? priceTarget.TargetConsensus : 0;
                stock.Upside = stock.Price > 0 ? Math.Round((stock.PriceTarget.Value - stock.Price) / stock.Price * 100, 1) : 0;   
            }
            await _stockRepository.UpdateAsync(stock);

            return true;
        }

        private double CalculateRSI(double[] closes, int period = 14)
        {
            var prices = closes.Reverse().ToArray(); // oldest first
            if (prices.Length < period + 1) return 50;
            double[] delta = new double[prices.Length - 1];
            for (int i = 0; i < delta.Length; i++) delta[i] = prices[i + 1] - prices[i];
            double avgGain = delta.Take(period).Where(d => d > 0).Sum() / period;
            double avgLoss = -delta.Take(period).Where(d => d < 0).Sum() / period;
            for (int i = period; i < delta.Length; i++)
            {
                avgGain = (avgGain * (period - 1) + Math.Max(delta[i], 0)) / period;
                avgLoss = (avgLoss * (period - 1) + Math.Max(-delta[i], 0)) / period;
            }
            if (avgLoss == 0) return 100;
            return 100 - 100 / (1 + avgGain / avgLoss);
        }

        private double CalculateAnnualVol(double[] closes)
        {
            var prices = closes.Reverse().ToArray(); // oldest first
            if (prices.Length < 2) return 0;
            double[] logReturns = new double[prices.Length - 1];
            for (int i = 0; i < logReturns.Length; i++)
                logReturns[i] = Math.Log(prices[i + 1] / prices[i]);
            double mean = logReturns.Average();
            double std = Math.Sqrt(logReturns.Sum(r => Math.Pow(r - mean, 2)) / (logReturns.Length - 1));
            return std * Math.Sqrt(252) * 100;
        }

        private double CalculateConsensusTrend(List<HistoricalGrade> gradeEntries) {
            var sorted = gradeEntries.OrderByDescending(e => e.Date).ToList();
            double[] scores = sorted.Take(6).Select(e =>
                (e.AnalystRatingsStrongBuy * 2.0 + e.AnalystRatingsBuy - e.AnalystRatingsSell - e.AnalystRatingsStrongSell * 2.0) /
                (e.AnalystRatingsStrongBuy + e.AnalystRatingsBuy + e.AnalystRatingsHold + e.AnalystRatingsSell + e.AnalystRatingsStrongSell)
            ).ToArray();
            double trend = Math.Round((scores[0] - scores[5]) * 10, 1);

            return trend;
        }
    }
}
