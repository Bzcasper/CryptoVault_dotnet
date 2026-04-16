using CryptoVault.Application.DTOs;

namespace CryptoVault.Application.Interfaces
{
    /// <summary>
    /// Abstraction over the Binance public REST API.
    /// All methods are read-only and require no authentication.
    /// </summary>
    public interface IBinanceApiClient
    {
        /// <summary>
        /// Gets the current price for a specific trading pair.
        /// </summary>
        Task<decimal> GetPriceAsync(string symbol);

        /// <summary>
        /// Gets current prices for all USDT trading pairs.
        /// </summary>
        Task<Dictionary<string, decimal>> GetAllPricesAsync();

        /// <summary>
        /// Gets 24h ticker statistics for a specific trading pair.
        /// </summary>
        Task<MarketAssetDto> Get24hTickerAsync(string symbol);

        /// <summary>
        /// Gets 24h ticker statistics for all USDT pairs.
        /// </summary>
        Task<List<MarketAssetDto>> GetAll24hTickersAsync();

        /// <summary>
        /// Searches available trading pairs matching a query.
        /// </summary>
        Task<List<MarketAssetDto>> SearchAssetsAsync(string query);

        /// <summary>
        /// Gets candlestick (OHLCV) data for charting.
        /// </summary>
        Task<List<CandlestickDto>> GetKlinesAsync(string symbol, string interval = "1d", int limit = 30);
    }
}
