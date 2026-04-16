using System.Text.Json.Serialization;

namespace CryptoVault.Infrastructure.ExternalApi
{
    /// <summary>
    /// Raw JSON deserialization models matching Binance API response shapes.
    /// These are internal infrastructure models — never exposed to the UI.
    /// </summary>

    // GET /api/v3/ticker/price
    public class BinancePriceResponse
    {
        [JsonPropertyName("symbol")]
        public string Symbol { get; set; }

        [JsonPropertyName("price")]
        public string Price { get; set; }
    }

    // GET /api/v3/ticker/24hr
    public class Binance24hTickerResponse
    {
        [JsonPropertyName("symbol")]
        public string Symbol { get; set; }

        [JsonPropertyName("priceChange")]
        public string PriceChange { get; set; }

        [JsonPropertyName("priceChangePercent")]
        public string PriceChangePercent { get; set; }

        [JsonPropertyName("weightedAvgPrice")]
        public string WeightedAvgPrice { get; set; }

        [JsonPropertyName("lastPrice")]
        public string LastPrice { get; set; }

        [JsonPropertyName("highPrice")]
        public string HighPrice { get; set; }

        [JsonPropertyName("lowPrice")]
        public string LowPrice { get; set; }

        [JsonPropertyName("volume")]
        public string Volume { get; set; }

        [JsonPropertyName("quoteVolume")]
        public string QuoteVolume { get; set; }

        [JsonPropertyName("openTime")]
        public long OpenTime { get; set; }

        [JsonPropertyName("closeTime")]
        public long CloseTime { get; set; }
    }

    // GET /api/v3/exchangeInfo (simplified)
    public class BinanceExchangeInfoResponse
    {
        [JsonPropertyName("symbols")]
        public List<BinanceSymbolInfo> Symbols { get; set; }
    }

    public class BinanceSymbolInfo
    {
        [JsonPropertyName("symbol")]
        public string Symbol { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("baseAsset")]
        public string BaseAsset { get; set; }

        [JsonPropertyName("quoteAsset")]
        public string QuoteAsset { get; set; }
    }

    // WebSocket mini ticker stream
    public class BinanceMiniTickerStream
    {
        [JsonPropertyName("e")]
        public string EventType { get; set; }

        [JsonPropertyName("s")]
        public string Symbol { get; set; }

        [JsonPropertyName("c")]
        public string ClosePrice { get; set; }

        [JsonPropertyName("o")]
        public string OpenPrice { get; set; }

        [JsonPropertyName("h")]
        public string HighPrice { get; set; }

        [JsonPropertyName("l")]
        public string LowPrice { get; set; }

        [JsonPropertyName("v")]
        public string TotalTradedBaseAssetVolume { get; set; }

        [JsonPropertyName("q")]
        public string TotalTradedQuoteAssetVolume { get; set; }
    }
}
