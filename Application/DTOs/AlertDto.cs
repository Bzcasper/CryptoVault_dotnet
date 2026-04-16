using CryptoVault.Domain.Enums;

namespace CryptoVault.Application.DTOs
{
    /// <summary>
    /// DTO for alert display.
    /// </summary>
    public class AlertDto
    {
        public int Id { get; set; }
        public string Symbol { get; set; }
        public AlertType Type { get; set; }
        public decimal TargetPrice { get; set; }
        public decimal CurrentPrice { get; set; }
        public bool IsTriggered { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? TriggeredAt { get; set; }

        public string TypeDisplay => Type == AlertType.PriceAbove ? "Price Above" : "Price Below";
        public decimal DistancePercent => CurrentPrice > 0 ? ((TargetPrice - CurrentPrice) / CurrentPrice) * 100m : 0m;
    }
}
