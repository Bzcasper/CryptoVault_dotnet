using System.ComponentModel.DataAnnotations;
using CryptoVault.Domain.Enums;

namespace CryptoVault.Domain.Entities
{
    /// <summary>
    /// Represents a price alert that triggers when a target price threshold is reached.
    /// </summary>
    public class Alert
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string Symbol { get; set; }

        /// <summary>
        /// The type of alert: PriceAbove or PriceBelow.
        /// </summary>
        [Required]
        public AlertType Type { get; set; }

        /// <summary>
        /// The target price threshold (USD).
        /// </summary>
        [Required]
        [Range(0.00000001, double.MaxValue, ErrorMessage = "Target price must be greater than zero.")]
        public decimal TargetPrice { get; set; }

        /// <summary>
        /// Whether the alert has been triggered.
        /// </summary>
        public bool IsTriggered { get; set; } = false;

        /// <summary>
        /// Whether the alert is currently active and being monitored.
        /// </summary>
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? TriggeredAt { get; set; }

        // ====== EF Core Relationships ======

        [Required]
        public int PortfolioId { get; set; }
        public Portfolio Portfolio { get; set; }
    }
}
