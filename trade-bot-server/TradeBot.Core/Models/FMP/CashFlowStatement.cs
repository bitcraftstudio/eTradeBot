using System;
using System.Collections.Generic;
using System.Text;

namespace TradeBot.Core.Models.FMP
{
    public class CashFlowStatement
    {
        public string Id { get; set; } = string.Empty;
        public string Symbol { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string ReportedCurrency { get; set; } = string.Empty;
        public string FiscalYear { get; set; } = string.Empty;
        public string Period { get; set; } = string.Empty;
        public decimal? NetIncome { get; set; }
        public decimal? DepreciationAndAmortization { get; set; }
        public decimal? DeferredIncomeTax { get; set; }
        public decimal? StockBasedCompensation { get; set; }
        public decimal? ChangeInWorkingCapital { get; set; }
        public decimal? AccountsReceivables { get; set; }
        public decimal? Inventory { get; set; }
        public decimal? AccountsPayables { get; set; }
        public decimal? OtherWorkingCapital { get; set; }
        public decimal? OtherNonCashItems { get; set; }
        public decimal? NetCashProvidedByOperatingActivities { get; set; }
        public decimal? InvestmentsInPropertyPlantAndEquipment { get; set; }
        public decimal? AcquisitionsNet { get; set; }
        public decimal? PurchasesOfInvestments { get; set; }
        public decimal? SalesMaturitiesOfInvestments { get; set; }
        public decimal? OtherInvestingActivities { get; set; }
        public decimal? NetCashProvidedByInvestingActivities { get; set; }
        public decimal? NetDebtIssuance { get; set; }
        public decimal? LongTermNetDebtIssuance { get; set; }
        public decimal? ShortTermNetDebtIssuance { get; set; }
        public decimal? NetStockIssuance { get; set; }
        public decimal? NetCommonStockIssuance { get; set; }
        public decimal? CommonStockIssuance { get; set; }
        public decimal? CommonStockRepurchased { get; set; }
        public decimal? NetPreferredStockIssuance { get; set; }
        public decimal? NetDividendsPaid { get; set; }
        public decimal? CommonDividendsPaid { get; set; }
        public decimal? PreferredDividendsPaid { get; set; }
        public decimal? OtherFinancingActivities { get; set; }
        public decimal? NetCashProvidedByFinancingActivities { get; set; }
        public decimal? EffectOfForexChangesOnCash { get; set; }
        public decimal? NetChangeInCash { get; set; }
        public decimal? CashAtEndOfPeriod { get; set; }
        public decimal? CashAtBeginningOfPeriod { get; set; }
        public decimal? OperatingCashFlow { get; set; }
        public decimal? CapitalExpenditure { get; set; }
        public decimal? FreeCashFlow { get; set; }
        public decimal? IncomeTaxesPaid { get; set; }
        public decimal? InterestPaid { get; set; }
    }
}