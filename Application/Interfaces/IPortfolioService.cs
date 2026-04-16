using CryptoVault.Application.DTOs;
using CryptoVault.Domain.Entities;

namespace CryptoVault.Application.Interfaces
{
    /// <summary>
    /// Service interface for portfolio management operations.
    /// </summary>
    public interface IPortfolioService
    {
        Task<Portfolio> GetOrCreateDefaultPortfolioAsync();
        Task<Portfolio> GetPortfolioByIdAsync(int id);
        Task UpdateBudgetAsync(int portfolioId, decimal newBudget);
        Task<PortfolioDashboardDto> GetDashboardDataAsync(int portfolioId);
        Task<decimal> GetTotalPortfolioValueAsync(int portfolioId);
    }
}
