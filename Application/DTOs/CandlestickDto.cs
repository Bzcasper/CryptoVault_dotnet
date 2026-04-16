namespace CryptoVault.Application.DTOs
{
    /// <summary>
    /// DTO representing an OHLCV candlestick data point for chart rendering.
    /// </summary>
    public class CandlestickDto
    {
        public DateTime OpenTime { get; set; }
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public decimal Volume { get; set; }
        public DateTime CloseTime { get; set; }
    }
}
