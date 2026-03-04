using System;
using System.Collections.Generic;
using System.Text;

namespace TradeBot.Core.Models.FMP
{
    public class IncomeStatement
    {
        public string Id { get; set; } = string.Empty;
        public string Symbol { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string ReportedCurrency { get; set; } = string.Empty;
        public string FiscalYear{ get; set; } = string.Empty;
        public string Period{ get; set; } = string.Empty;
        public decimal? Revenue { get; set; }
        public decimal? CostOfRevenue { get; set; }
        public decimal? GrossProfit { get; set; }
        public decimal? ResearchAndDevelopmentExpenses { get; set; }
        public decimal? GeneralAndAdministrativeExpenses { get; set; }
        public decimal? SellingAndMarketingExpenses { get; set; }
        public decimal? SellingGeneralAndAdministrativeExpenses { get; set; }
        public decimal? OtherExpenses { get; set; }
        public decimal? OperatingExpenses { get; set; }
        public decimal? CostAndExpenses { get; set; }
        public decimal? NetInterestIncome { get; set; }
        public decimal? InterestIncome { get; set; }
        public decimal? InterestExpense { get; set; }
        public decimal? DepreciationAndAmortization { get; set; }
        public decimal? Ebitda { get; set; }
        public decimal? Ebit { get; set; }
        public decimal? NonOperatingIncomeExcludingInterest { get; set; }
        public decimal? OperatingIncome { get; set; }
        public decimal? TotalOtherIncomeExpensesNet { get; set; }
        public decimal? IncomeBeforeTax { get; set; }
        public decimal? IncomeTaxExpense { get; set; }
        public decimal? NetIncomeFromContinuingOperations { get; set; }
        public decimal? NetIncomeFromDiscontinuedOperations { get; set; }
        public decimal? OtherAdjustmentsToNetIncome { get; set; }
        public decimal? NetIncomeDeductions { get; set; }
        public decimal? BottomLineNetIncome { get; set; }
        public decimal? Eps { get; set; }
        public decimal? EpsDiluted { get; set; }
        public decimal? WeightedAverageShsOut { get; set; }
        public decimal? WeightedAverageShsOutDil { get; set; }
    }
}
