using CryptoVault.Domain.Entities;

namespace CryptoVault.Application.Interfaces
{
    /// <summary>
    /// Service interface for watchlist management.
    /// </summary>
    public interface IWatchlistService
    {
        Task<List<WatchlistItem>> GetWatchlistAsync(int portfolioId);
        Task<WatchlistItem> AddToWatchlistAsync(int portfolioId, string symbol, string name, string imageUrl);
        Task RemoveFromWatchlistAsync(int id);
        Task RemoveFromWatchlistBySymbolAsync(int portfolioId, string symbol);
        Task<bool> IsOnWatchlistAsync(int portfolioId, string symbol);
    }
}
