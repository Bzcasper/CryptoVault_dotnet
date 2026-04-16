using CryptoVault.Application.Interfaces;
using CryptoVault.Domain.Entities;
using CryptoVault.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CryptoVault.Application.Services
{
    /// <summary>
    /// Implements watchlist CRUD operations.
    /// </summary>
    public class WatchlistService : IWatchlistService
    {
        private readonly AppDbContext _db;

        public WatchlistService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<WatchlistItem>> GetWatchlistAsync(int portfolioId)
        {
            return await _db.WatchlistItems
                .Where(w => w.PortfolioId == portfolioId)
                .OrderByDescending(w => w.AddedAt)
                .ToListAsync();
        }

        public async Task<WatchlistItem> AddToWatchlistAsync(int portfolioId, string symbol, string name, string imageUrl)
        {
            // Check if already on watchlist
            var existing = await _db.WatchlistItems
                .FirstOrDefaultAsync(w => w.PortfolioId == portfolioId && w.Symbol == symbol);

            if (existing != null)
                return existing;

            var item = new WatchlistItem
            {
                Symbol = symbol,
                Name = name,
                ImageUrl = imageUrl,
                PortfolioId = portfolioId,
                AddedAt = DateTime.UtcNow
            };

            _db.WatchlistItems.Add(item);
            await _db.SaveChangesAsync();
            return item;
        }

        public async Task RemoveFromWatchlistAsync(int id)
        {
            var item = await _db.WatchlistItems.FindAsync(id);
            if (item != null)
            {
                _db.WatchlistItems.Remove(item);
                await _db.SaveChangesAsync();
            }
        }

        public async Task RemoveFromWatchlistBySymbolAsync(int portfolioId, string symbol)
        {
            var item = await _db.WatchlistItems
                .FirstOrDefaultAsync(w => w.PortfolioId == portfolioId && w.Symbol == symbol);
            if (item != null)
            {
                _db.WatchlistItems.Remove(item);
                await _db.SaveChangesAsync();
            }
        }

        public async Task<bool> IsOnWatchlistAsync(int portfolioId, string symbol)
        {
            return await _db.WatchlistItems
                .AnyAsync(w => w.PortfolioId == portfolioId && w.Symbol == symbol);
        }
    }
}
