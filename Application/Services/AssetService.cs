using CryptoVault.Application.DTOs;
using CryptoVault.Application.Interfaces;
using CryptoVault.Domain.Entities;
using CryptoVault.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CryptoVault.Application.Services
{
    /// <summary>
    /// Implements asset management with weighted average buy price recalculation.
    /// </summary>
    public class AssetService : IAssetService
    {
        private readonly AppDbContext _db;
        private readonly IMarketDataService _marketData;
        private readonly ILogger<AssetService> _logger;

        public AssetService(AppDbContext db, IMarketDataService marketData, ILogger<AssetService> logger)
        {
            _db = db;
            _marketData = marketData;
            _logger = logger;
        }

        public async Task<List<Asset>> GetAssetsAsync(int portfolioId)
        {
            return await _db.Assets
                .Where(a => a.PortfolioId == portfolioId && a.Quantity > 0)
                .OrderByDescending(a => a.TotalInvested)
                .ToListAsync();
        }

        public async Task<Asset> GetAssetBySymbolAsync(int portfolioId, string symbol)
        {
            return await _db.Assets
                .FirstOrDefaultAsync(a => a.PortfolioId == portfolioId && a.Symbol == symbol);
        }

        public async Task<Asset> GetAssetByIdAsync(int id)
        {
            return await _db.Assets.FindAsync(id);
        }

        public async Task<List<AssetDto>> GetAssetDtosAsync(int portfolioId)
        {
            var assets = await GetAssetsAsync(portfolioId);
            var dtos = new List<AssetDto>();
            decimal totalValue = 0m;

            foreach (var asset in assets)
            {
                var marketData = await _marketData.GetMarketDataAsync(asset.Symbol);
                var currentPrice = marketData?.Price ?? await _marketData.GetCurrentPriceAsync(asset.Symbol);

                var dto = new AssetDto
                {
                    Id = asset.Id,
                    Symbol = asset.Symbol,
                    Name = asset.Name,
                    ImageUrl = asset.ImageUrl,
                    Quantity = asset.Quantity,
                    AverageBuyPrice = asset.AverageBuyPrice,
                    TotalInvested = asset.TotalInvested,
                    CurrentPrice = currentPrice,
                    Change24hPercent = marketData?.PriceChangePercent ?? 0m,
                    Volume24h = marketData?.QuoteVolume ?? 0m
                };

                dtos.Add(dto);
                totalValue += dto.CurrentValue;
            }

            // Compute allocation percentages
            foreach (var dto in dtos)
            {
                dto.AllocationPercent = totalValue > 0 ? (dto.CurrentValue / totalValue) * 100m : 0m;
            }

            return dtos.OrderByDescending(d => d.CurrentValue).ToList();
        }

        public async Task<Asset> AddOrUpdateAssetAsync(int portfolioId, string symbol, string name, string imageUrl, decimal quantity, decimal price)
        {
            var existing = await GetAssetBySymbolAsync(portfolioId, symbol);

            if (existing != null)
            {
                // Recalculate weighted average buy price
                var totalOldCost = existing.AverageBuyPrice * existing.Quantity;
                var newCost = price * quantity;
                var newTotalQuantity = existing.Quantity + quantity;

                existing.AverageBuyPrice = newTotalQuantity > 0
                    ? (totalOldCost + newCost) / newTotalQuantity
                    : 0m;

                existing.Quantity = newTotalQuantity;
                existing.TotalInvested += newCost;
                existing.UpdatedAt = DateTime.UtcNow;

                await _db.SaveChangesAsync();
                _logger.LogInformation("Updated asset {Symbol}: qty={Qty}, avgPrice={AvgPrice}",
                    symbol, existing.Quantity, existing.AverageBuyPrice);

                return existing;
            }
            else
            {
                var asset = new Asset
                {
                    Symbol = symbol,
                    Name = name,
                    ImageUrl = imageUrl,
                    Quantity = quantity,
                    AverageBuyPrice = price,
                    TotalInvested = quantity * price,
                    PortfolioId = portfolioId,
                    FirstBoughtAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _db.Assets.Add(asset);
                await _db.SaveChangesAsync();
                _logger.LogInformation("Added new asset {Symbol} to portfolio {PortfolioId}", symbol, portfolioId);

                return asset;
            }
        }

        public async Task ReduceAssetAsync(int assetId, decimal quantity)
        {
            var asset = await _db.Assets.FindAsync(assetId);
            if (asset == null) return;

            asset.Quantity -= quantity;
            if (asset.Quantity <= 0)
            {
                asset.Quantity = 0;
                asset.TotalInvested = 0;
            }
            else
            {
                // Proportionally reduce TotalInvested
                var ratio = quantity / (asset.Quantity + quantity);
                asset.TotalInvested -= asset.TotalInvested * ratio;
            }

            asset.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        public async Task RemoveAssetAsync(int assetId)
        {
            var asset = await _db.Assets.FindAsync(assetId);
            if (asset != null)
            {
                _db.Assets.Remove(asset);
                await _db.SaveChangesAsync();
            }
        }
    }
}
