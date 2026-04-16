using System.ComponentModel.DataAnnotations;

namespace CryptoVault.Domain.Entities
{
    /// <summary>
    /// Represents an asset on the user's watchlist (tracked without owning).
    /// </summary>
    public class WatchlistItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string Symbol { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string ImageUrl { get; set; }

        public DateTime AddedAt { get; set; } = DateTime.UtcNow;

        // ====== EF Core Relationships ======

        [Required]
        public int PortfolioId { get; set; }
        public Portfolio Portfolio { get; set; }
    }
}
