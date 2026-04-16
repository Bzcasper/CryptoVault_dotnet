using System.ComponentModel.DataAnnotations;
using CryptoVault.Domain.Enums;

namespace CryptoVault.Domain.Entities
{
    /// <summary>
    /// Represents a buy or sell transaction within a portfolio.
    /// Each transaction records the exact price, quantity, and fee at time of execution.
    /// </summary>
    public class Transaction
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Trading symbol (e.g., "BTCUSDT").
        /// </summary>
        [Required]
        [StringLength(20)]
        public string Symbol { get; set; }

        /// <summary>
        /// Human-readable name (e.g., "Bitcoin").
        /// </summary>
        [Required]
        [StringLength(100)]
        public string AssetName { get; set; }

        /// <summary>
        /// Buy or Sell.
        /// </summary>
        [Required]
        public TransactionType Type { get; set; }

        /// <summary>
        /// Quantity of asset traded.
        /// </summary>
        [Required]
        [Range(0.00000001, double.MaxValue, ErrorMessage = "Quantity must be greater than zero.")]
        public decimal Quantity { get; set; }

        /// <summary>
        /// Price per unit at execution time (from Binance API).
        /// </summary>
        [Required]
        [Range(0.00000001, double.MaxValue)]
        public decimal PricePerUnit { get; set; }

        /// <summary>
        /// Total cost/revenue = Quantity × PricePerUnit.
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Simulated trading fee (0.1% like Binance).
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal Fee { get; set; }

        /// <summary>
        /// Optional notes about the transaction.
        /// </summary>
        [StringLength(500)]
        public string Notes { get; set; }

        public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;

        // ====== EF Core Relationships ======

        /// <summary>
        /// Foreign key to Portfolio (1-to-N).
        /// </summary>
        [Required]
        public int PortfolioId { get; set; }
        public Portfolio Portfolio { get; set; }

        /// <summary>
        /// Foreign key to Asset (1-to-N). Nullable for cases where asset is fully sold.
        /// </summary>
        public int? AssetId { get; set; }
        public Asset Asset { get; set; }
    }
}
