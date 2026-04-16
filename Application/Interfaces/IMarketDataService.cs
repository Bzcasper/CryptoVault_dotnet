using CryptoVault.Application.DTOs;

namespace CryptoVault.Application.Interfaces
{
    /// <summary>
    /// Orchestrates market data retrieval with caching.
    /// Combines API calls with in-memory cache for performance.
    /// </summary>
    public interface IMarketDataService
    {
        /// <summary>
        /// Gets the current price for a symbol (uses cache if available).
        /// </summary>
        Task<decimal> GetCurrentPriceAsync(string symbol);

        /// <summary>
        /// Gets all cached prices (from WebSocket stream or REST fallback).
        /// </summary>
        Dictionary<string, decimal> GetCachedPrices();

        /// <summary>
        /// Gets 24h market data for a symbol.
        /// </summary>
        Task<MarketAssetDto> GetMarketDataAsync(string symbol);

        /// <summary>
        /// Searches for assets matching a query.
        /// </summary>
        Task<List<MarketAssetDto>> SearchAssetsAsync(string query);

        /// <summary>
        /// Gets chart data for a symbol.
        /// </summary>
        Task<List<CandlestickDto>> GetChartDataAsync(string symbol, string interval = "1d", int limit = 30);

        /// <summary>
        /// Gets the top N assets by 24h volume.
        /// </summary>
        Task<List<MarketAssetDto>> GetTopAssetsAsync(int count = 20);

        /// <summary>
        /// Gets all tradable assets with rank and category assigned.
        /// </summary>
        Task<List<MarketAssetDto>> GetAllMarketAssetsAsync();
    }
}
