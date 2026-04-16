using CryptoVault.Application.DTOs;
using CryptoVault.Domain.Entities;

namespace CryptoVault.Application.Interfaces
{
    /// <summary>
    /// Service interface for asset management operations.
    /// </summary>
    public interface IAssetService
    {
        Task<List<Asset>> GetAssetsAsync(int portfolioId);
        Task<Asset> GetAssetBySymbolAsync(int portfolioId, string symbol);
        Task<Asset> GetAssetByIdAsync(int id);
        Task<List<AssetDto>> GetAssetDtosAsync(int portfolioId);
        Task<Asset> AddOrUpdateAssetAsync(int portfolioId, string symbol, string name, string imageUrl, decimal quantity, decimal price);
        Task ReduceAssetAsync(int assetId, decimal quantity);
        Task RemoveAssetAsync(int assetId);
    }
}
