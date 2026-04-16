using System.ComponentModel.DataAnnotations;

namespace CryptoVault.Domain.Entities
{
    /// <summary>
    /// Represents a cryptocurrency asset held in a portfolio.
    /// Tracks quantity, average buy price, and total invested amount.
    /// </summary>
    public class Asset
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Trading symbol (e.g., "BTCUSDT", "ETHUSDT").
        /// </summary>
        [Required]
        [StringLength(20)]
        public string Symbol { get; set; }

        /// <summary>
        /// Human-readable name (e.g., "Bitcoin", "Ethereum").
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        /// <summary>
        /// URL to the asset's icon/logo.
        /// </summary>
        [StringLength(500)]
        public string ImageUrl { get; set; }

        /// <summary>
        /// Total quantity currently held.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal Quantity { get; set; }

        /// <summary>
        /// Weighted average buy price across all buy transactions.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal AverageBuyPrice { get; set; }

        /// <summary>
        /// Total USD amount invested in this asset (sum of all buy amounts minus sells).
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal TotalInvested { get; set; }

        public DateTime FirstBoughtAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // ====== EF Core Relationships ======

        /// <summary>
        /// Foreign key to the portfolio (1-to-N): Each asset belongs to one portfolio.
        /// </summary>
        [Required]
        public int PortfolioId { get; set; }
        public Portfolio Portfolio { get; set; }

        /// <summary>
        /// One-to-Many: An asset can have multiple transactions.
        /// </summary>
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
