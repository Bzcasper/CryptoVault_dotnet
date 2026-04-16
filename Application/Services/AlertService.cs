using CryptoVault.Application.Interfaces;
using CryptoVault.Domain.Entities;
using CryptoVault.Domain.Enums;
using CryptoVault.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CryptoVault.Application.Services
{
    /// <summary>
    /// Implements alert management with trigger checking logic.
    /// </summary>
    public class AlertService : IAlertService
    {
        private readonly AppDbContext _db;
        private readonly ILogger<AlertService> _logger;

        public AlertService(AppDbContext db, ILogger<AlertService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<List<Alert>> GetActiveAlertsAsync(int portfolioId)
        {
            return await _db.Alerts
                .Where(a => a.PortfolioId == portfolioId && a.IsActive && !a.IsTriggered)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Alert>> GetTriggeredAlertsAsync(int portfolioId, int limit = 20)
        {
            return await _db.Alerts
                .Where(a => a.PortfolioId == portfolioId && a.IsTriggered)
                .OrderByDescending(a => a.TriggeredAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<List<Alert>> GetAllAlertsAsync(int portfolioId)
        {
            return await _db.Alerts
                .Where(a => a.PortfolioId == portfolioId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<Alert> CreateAlertAsync(int portfolioId, string symbol, AlertType type, decimal targetPrice)
        {
            var alert = new Alert
            {
                Symbol = symbol,
                Type = type,
                TargetPrice = targetPrice,
                PortfolioId = portfolioId,
                IsActive = true,
                IsTriggered = false,
                CreatedAt = DateTime.UtcNow
            };

            _db.Alerts.Add(alert);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Created alert: {Type} {Symbol} @ ${Price}", type, symbol, targetPrice);

            return alert;
        }

        public async Task DeleteAlertAsync(int id)
        {
            var alert = await _db.Alerts.FindAsync(id);
            if (alert != null)
            {
                _db.Alerts.Remove(alert);
                await _db.SaveChangesAsync();
            }
        }

        public async Task DeactivateAlertAsync(int id)
        {
            var alert = await _db.Alerts.FindAsync(id);
            if (alert != null)
            {
                alert.IsActive = false;
                await _db.SaveChangesAsync();
            }
        }

        public async Task<List<Alert>> CheckAndTriggerAlertsAsync(Dictionary<string, decimal> currentPrices)
        {
            var triggered = new List<Alert>();

            var activeAlerts = await _db.Alerts
                .Where(a => a.IsActive && !a.IsTriggered)
                .ToListAsync();

            foreach (var alert in activeAlerts)
            {
                if (!currentPrices.TryGetValue(alert.Symbol, out var currentPrice))
                    continue;

                bool shouldTrigger = alert.Type switch
                {
                    AlertType.PriceAbove => currentPrice >= alert.TargetPrice,
                    AlertType.PriceBelow => currentPrice <= alert.TargetPrice,
                    _ => false
                };

                if (shouldTrigger)
                {
                    alert.IsTriggered = true;
                    alert.IsActive = false;
                    alert.TriggeredAt = DateTime.UtcNow;
                    triggered.Add(alert);
                    _logger.LogInformation("Alert triggered: {Symbol} {Type} @ ${Target} (current: ${Current})",
                        alert.Symbol, alert.Type, alert.TargetPrice, currentPrice);
                }
            }

            if (triggered.Any())
                await _db.SaveChangesAsync();

            return triggered;
        }
    }
}
