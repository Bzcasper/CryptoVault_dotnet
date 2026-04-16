using CryptoVault.Domain.Enums;

namespace CryptoVault.Application.DTOs
{
    /// <summary>
    /// DTO for transaction display with computed profit/loss per trade.
    /// </summary>
    public class TransactionDto
    {
        public int Id { get; set; }
        public string Symbol { get; set; }
        public string AssetName { get; set; }
        public TransactionType Type { get; set; }
        public decimal Quantity { get; set; }
        public decimal PricePerUnit { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Fee { get; set; }
        public string Notes { get; set; }
        public DateTime ExecutedAt { get; set; }

        /// <summary>
        /// Current market price (for computing unrealized P&L on buys).
        /// </summary>
        public decimal CurrentPrice { get; set; }

        /// <summary>
        /// P&L for this transaction based on current price.
        /// Buy: (CurrentPrice - PricePerUnit) × Quantity - Fee
        /// Sell: (PricePerUnit - AvgBuyPrice) × Quantity - Fee (realized)
        /// </summary>
        public decimal ProfitLoss { get; set; }

        public string TypeDisplay => Type == TransactionType.Buy ? "BUY" : "SELL";
        public string TypeColor => Type == TransactionType.Buy ? "#0ECB81" : "#F6465D";
    }
}
