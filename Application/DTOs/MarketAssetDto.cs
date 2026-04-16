namespace CryptoVault.Application.DTOs
{
    /// <summary>
    /// DTO for market asset search results from Binance API.
    /// Represents a tradeable cryptocurrency pair with 24h statistics.
    /// </summary>
    public class MarketAssetDto
    {
        public string Symbol { get; set; }
        public string BaseAsset { get; set; }
        public string QuoteAsset { get; set; }
        public string DisplayName { get; set; }
        public string ImageUrl { get; set; }

        // ====== Price Data ======
        public decimal Price { get; set; }
        public decimal PriceChangePercent { get; set; }
        public decimal PriceChange { get; set; }
        public decimal HighPrice { get; set; }
        public decimal LowPrice { get; set; }
        public decimal Volume { get; set; }
        public decimal QuoteVolume { get; set; }

        /// <summary>
        /// Whether this asset is available for trading (filters out deprecated pairs).
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Category for market page filtering (Layer1, DeFi, Meme, etc.).
        /// </summary>
        public string Category { get; set; } = "Other";

        /// <summary>
        /// Display rank based on volume/market cap.
        /// </summary>
        public int Rank { get; set; }
    }
}
