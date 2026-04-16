namespace CryptoVault.Application.DTOs
{
    /// <summary>
    /// DTO combining held asset data with real-time market information.
    /// Used for rendering the Assets page and Dashboard.
    /// </summary>
    public class AssetDto
    {
        public int Id { get; set; }
        public string Symbol { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public decimal Quantity { get; set; }
        public decimal AverageBuyPrice { get; set; }
        public decimal TotalInvested { get; set; }

        // ====== Real-time data (enriched from Binance API) ======
        public decimal CurrentPrice { get; set; }
        public decimal CurrentValue => Quantity * CurrentPrice;
        public decimal ProfitLoss => CurrentValue - TotalInvested;
        public decimal ProfitLossPercent => TotalInvested > 0 ? (ProfitLoss / TotalInvested) * 100m : 0m;
        public decimal Change24hPercent { get; set; }
        public decimal Volume24h { get; set; }

        /// <summary>
        /// Allocation percentage within the portfolio (set by service).
        /// </summary>
        public decimal AllocationPercent { get; set; }
    }
}
