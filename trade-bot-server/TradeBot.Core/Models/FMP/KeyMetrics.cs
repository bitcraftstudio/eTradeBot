using System;
using System.Collections.Generic;
using System.Text;

namespace TradeBot.Core.Models.FMP
{
    public class KeyMetrics
    {
        public string Id { get; set; } = string.Empty;
        public string Symbol { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string ReportedCurrency { get; set; } = string.Empty;
        public string FiscalYear { get; set; } = string.Empty;
        public string Period { get; set; } = string.Empty;
        public decimal? MarketCap { get; set; }
        public decimal? EnterpriseValue { get; set; }
        public decimal? EvToSales { get; set; }
        public decimal? EvToOperatingCashFlow { get; set; }
        public decimal? EvToFreeCashFlow { get; set; }
        public decimal? EvToEBITDA { get; set; }
        public decimal? NetDebtToEBITDA { get; set; }
        public decimal? CurrentRatio { get; set; }
        public decimal? IncomeQuality { get; set; }
        public decimal? GrahamNumber { get; set; }
        public decimal? GrahamNetNet { get; set; }
        public decimal? TaxBurden { get; set; }
        public decimal? InterestBurden { get; set; }
        public decimal? WorkingCapital { get; set; }
        public decimal? InvestedCapital { get; set; }
        public decimal? ReturnOnAssets { get; set; }
        public decimal? OperatingReturnOnAssets { get; set; }
        public decimal? ReturnOnTangibleAssets { get; set; }
        public decimal? ReturnOnEquity { get; set; }
        public decimal? ReturnOnInvestedCapital { get; set; }
        public decimal? ReturnOnCapitalEmployed { get; set; }
        public decimal? EarningsYield { get; set; }
        public decimal? FreeCashFlowYield { get; set; }
        public decimal? CapexToOperatingCashFlow { get; set; }
        public decimal? CapexToDepreciation { get; set; }
        public decimal? CapexToRevenue { get; set; }
        public decimal? SalesGeneralAndAdministrativeToRevenue { get; set; }
        public decimal? ResearchAndDevelopementToRevenue { get; set; }
        public decimal? StockBasedCompensationToRevenue { get; set; }
        public decimal? IntangiblesToTotalAssets { get; set; }
        public decimal? AverageReceivables { get; set; }
        public decimal? AveragePayables { get; set; }
        public decimal? AverageInventory { get; set; }
        public decimal? DaysOfSalesOutstanding { get; set; }
        public decimal? DaysOfPayablesOutstanding { get; set; }
        public decimal? DaysOfInventoryOutstanding { get; set; }
        public decimal? OperatingCycle { get; set; }
        public decimal? CashConversionCycle { get; set; }
        public decimal? FreeCashFlowToEquity { get; set; }
        public decimal? FreeCashFlowToFirm { get; set; }
        public decimal? TangibleAssetValue { get; set; }
        public decimal? NetCurrentAssetValue { get; set; }
    }
}