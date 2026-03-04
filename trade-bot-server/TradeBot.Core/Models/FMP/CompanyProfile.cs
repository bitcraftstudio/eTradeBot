namespace TradeBot.Core.Models.FMP
{
    public class CompanyProfile
    {
        public string Symbol { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal MarketCap { get; set; }
        public decimal Beta { get; set; }
        public decimal LastDividend { get; set; }
        public string Range { get; set; } = string.Empty;
        public decimal Change { get; set; }
        public decimal ChangePercent { get; set; }
        public decimal Volume { get; set; }
        public decimal AverageVolume { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
        public string Cik { get; set; } = string.Empty;
        public string Isin { get; set; } = string.Empty;
        public string Cusip { get; set; } = string.Empty;
        public string Exchange { get; set; } = string.Empty;
        public string ExchangeFullName { get; set; } = string.Empty;
        public string Industry { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Ceo { get; set; } = string.Empty;
        public string Sector { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string FullTimeEmployees { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Zip { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
        public string IpoDate { get; set; } = string.Empty;
        public bool IsEtf { get; set; }
        public bool IsActivelyTrading { get; set; }
        public bool IsFund { get; set; }
    }
}
