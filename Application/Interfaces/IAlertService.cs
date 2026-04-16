using CryptoVault.Application.DTOs;
using CryptoVault.Domain.Entities;
using CryptoVault.Domain.Enums;

namespace CryptoVault.Application.Interfaces
{
    /// <summary>
    /// Service interface for price alert management.
    /// </summary>
    public interface IAlertService
    {
        Task<List<Alert>> GetActiveAlertsAsync(int portfolioId);
        Task<List<Alert>> GetTriggeredAlertsAsync(int portfolioId, int limit = 20);
        Task<List<Alert>> GetAllAlertsAsync(int portfolioId);
        Task<Alert> CreateAlertAsync(int portfolioId, string symbol, AlertType type, decimal targetPrice);
        Task DeleteAlertAsync(int id);
        Task DeactivateAlertAsync(int id);
        Task<List<Alert>> CheckAndTriggerAlertsAsync(Dictionary<string, decimal> currentPrices);
    }
}
