using System.ComponentModel.DataAnnotations;

namespace CryptoVault.Domain.Entities
{
    /// <summary>
    /// Represents a point-in-time capture of portfolio value for historical charts.
    /// Snapshots are taken periodically by a background service.
    /// </summary>
    public class PriceSnapshot
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Trading symbol this snapshot belongs to.
        /// Use "PORTFOLIO" for total portfolio value snapshots.
        /// </summary>
        [Required]
        [StringLength(20)]
        public string Symbol { get; set; }

        /// <summary>
        /// Price at the time of capture (USD).
        /// </summary>
        [Required]
        public decimal Price { get; set; }

        /// <summary>
        /// 24h trading volume at capture time.
        /// </summary>
        public decimal Volume24h { get; set; }

        /// <summary>
        /// 24h price change percentage at capture time.
        /// </summary>
        public decimal ChangePercent24h { get; set; }

        public DateTime CapturedAt { get; set; } = DateTime.UtcNow;
    }
}
