using System.ComponentModel.DataAnnotations;

namespace CryptoVault.Domain.Entities
{
    /// <summary>
    /// Represents an investment portfolio with budget tracking.
    /// One portfolio contains multiple assets, transactions, watchlist items, and alerts.
    /// </summary>
    public class Portfolio
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Portfolio name must be between 2 and 100 characters.")]
        public string Name { get; set; }

        /// <summary>
        /// The starting budget allocated to this portfolio (in USD).
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "Initial budget must be a positive value.")]
        public decimal InitialBudget { get; set; }

        /// <summary>
        /// The current available budget (decreases on buy, increases on sell).
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal CurrentBudget { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // ====== Navigation Properties (EF Core Relationships) ======

        /// <summary>
        /// One-to-Many: A portfolio holds multiple assets.
        /// </summary>
        public ICollection<Asset> Assets { get; set; } = new List<Asset>();

        /// <summary>
        /// One-to-Many: A portfolio records multiple transactions.
        /// </summary>
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

        /// <summary>
        /// One-to-Many: A portfolio can have multiple watchlist items.
        /// </summary>
        public ICollection<WatchlistItem> WatchlistItems { get; set; } = new List<WatchlistItem>();

        /// <summary>
        /// One-to-Many: A portfolio can have multiple alerts.
        /// </summary>
        public ICollection<Alert> Alerts { get; set; } = new List<Alert>();
    }
}
