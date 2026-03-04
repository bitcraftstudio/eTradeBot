using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TradeBot.Core.Models.FMP
{
    public class StockQuote
    {
        public string Symbol { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal ChangePercent { get; set; }
        public decimal Change { get; set; }
        public long Volume { get; set; }
        public decimal DayLow { get; set; }
        public decimal DayHigh { get; set; }
        public decimal YearHigh { get; set; }
        public decimal YearLow { get; set; }
        public decimal MarketCap { get; set; }
        public decimal PriceAvg50 { get; set; }
        public decimal PriceAvg200 { get; set; }
        public string Exchange { get; set; } = string.Empty;
        public decimal Open { get; set; }
        public decimal PreviousClose { get; set; }
        
        [JsonConverter(typeof(UnixTimestampConverter))]
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// Custom JSON converter for Unix timestamps (long) to DateTime.
    /// Assumes timestamps are in seconds since Unix epoch (1970-01-01T00:00:00Z).
    /// </summary>
    public class UnixTimestampConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.Number)
            {
                throw new JsonException("Expected a number (Unix timestamp) for DateTime.");
            }

            long unixTime = reader.GetInt64();
            return DateTimeOffset.FromUnixTimeSeconds(unixTime).UtcDateTime;
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            long unixTime = ((DateTimeOffset)value).ToUnixTimeSeconds();
            writer.WriteNumberValue(unixTime);
        }
    }
}
