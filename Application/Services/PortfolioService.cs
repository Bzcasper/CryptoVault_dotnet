using CryptoVault.Application.DTOs;
using CryptoVault.Application.Interfaces;
using CryptoVault.Domain.Entities;
using CryptoVault.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CryptoVault.Application.Services
{
    /// <summary>
    /// Implements portfolio management operations including dashboard data aggregation.
    /// </summary>
    public class PortfolioService : IPortfolioService
    {
        private readonly AppDbContext _db;
        private readonly IMarketDataService _marketData;
        private readonly IConfiguration _config;
        private readonly ILogger<PortfolioService> _logger;

        // Colors for allocation chart
        private static readonly string[] ChartColors = {
            "#F0B90B", "#0ECB81", "#1E90FF", "#F6465D", "#B659FF",
            "#FF6B35", "#00D2FF", "#FFD700", "#FF1493", "#00FA9A",
            "#FF4500", "#7B68EE", "#20B2AA", "#FF69B4", "#32CD32"
        };

        public PortfolioService(AppDbContext db, IMarketDataService marketData, IConfiguration config, ILogger<PortfolioService> logger)
        {
            _db = db;
            _marketData = marketData;
            _config = config;
            _logger = logger;
        }

        public async Task<Portfolio> GetOrCreateDefaultPortfolioAsync()
        {
            var portfolio = await _db.Portfolios.FirstOrDefaultAsync();

            if (portfolio == null)
            {
                var initialBudget = decimal.Parse(_config["Portfolio:InitialBudget"] ?? "10000", System.Globalization.CultureInfo.InvariantCulture);
                var name = _config["Portfolio:DefaultName"] ?? "My Portfolio";

                portfolio = new Portfolio
                {
                    Name = name,
                    InitialBudget = initialBudget,
                    CurrentBudget = initialBudget,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _db.Portfolios.Add(portfolio);
                await _db.SaveChangesAsync();
                _logger.LogInformation("Created default portfolio '{Name}' with ${Budget}", name, initialBudget);
            }

            return portfolio;
        }

        public async Task<Portfolio> GetPortfolioByIdAsync(int id)
        {
            return await _db.Portfolios
                .Include(p => p.Assets)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task UpdateBudgetAsync(int portfolioId, decimal newBudget)
        {
            var portfolio = await _db.Portfolios.FindAsync(portfolioId);
            if (portfolio != null)
            {
                portfolio.CurrentBudget = newBudget;
                portfolio.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }
        }

        public async Task<decimal> GetTotalPortfolioValueAsync(int portfolioId)
        {
            var assets = await _db.Assets.Where(a => a.PortfolioId == portfolioId).ToListAsync();
            decimal totalValue = 0m;

            foreach (var asset in assets)
            {
                var price = await _marketData.GetCurrentPriceAsync(asset.Symbol);
                totalValue += asset.Quantity * price;
            }

            return totalValue;
        }

        public async Task<PortfolioDashboardDto> GetDashboardDataAsync(int portfolioId)
        {
            var portfolio = await _db.Portfolios
                .Include(p => p.Assets)
                .FirstOrDefaultAsync(p => p.Id == portfolioId);

            if (portfolio == null)
                return new PortfolioDashboardDto();

            var assetDtos = new List<AssetDto>();
            decimal totalPortfolioValue = 0m;

            // Enrich each asset with real-time market data
            foreach (var asset in portfolio.Assets.Where(a => a.Quantity > 0))
            {
                try
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

                    assetDtos.Add(dto);
                    totalPortfolioValue += dto.CurrentValue;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to fetch market data for {Symbol}", asset.Symbol);
                }
            }

            // Calculate allocation percentages
            foreach (var dto in assetDtos)
            {
                dto.AllocationPercent = totalPortfolioValue > 0
                    ? (dto.CurrentValue / totalPortfolioValue) * 100m
                    : 0m;
            }

            // Build allocation chart items
            var allocations = assetDtos
                .OrderByDescending(a => a.CurrentValue)
                .Select((a, i) => new AllocationChartItem
                {
                    Symbol = a.Symbol.Replace("USDT", ""),
                    Name = a.Name,
                    Value = a.CurrentValue,
                    Percentage = a.AllocationPercent,
                    Color = ChartColors[i % ChartColors.Length]
                })
                .ToList();

            // Recent transactions
            var recentTxns = await _db.Transactions
                .Where(t => t.PortfolioId == portfolioId)
                .OrderByDescending(t => t.ExecutedAt)
                .Take(10)
                .Select(t => new TransactionDto
                {
                    Id = t.Id,
                    Symbol = t.Symbol,
                    AssetName = t.AssetName,
                    Type = t.Type,
                    Quantity = t.Quantity,
                    PricePerUnit = t.PricePerUnit,
                    TotalAmount = t.TotalAmount,
                    Fee = t.Fee,
                    Notes = t.Notes,
                    ExecutedAt = t.ExecutedAt
                })
                .ToListAsync();

            var txnCount = await _db.Transactions.CountAsync(t => t.PortfolioId == portfolioId);

            return new PortfolioDashboardDto
            {
                PortfolioName = portfolio.Name,
                InitialBudget = portfolio.InitialBudget,
                AvailableBudget = portfolio.CurrentBudget,
                TotalInvested = assetDtos.Sum(a => a.TotalInvested),
                TotalPortfolioValue = totalPortfolioValue,
                TotalAssets = assetDtos.Count,
                TotalTransactions = txnCount,
                Assets = assetDtos.OrderByDescending(a => a.CurrentValue).ToList(),
                TopPerformers = assetDtos.OrderByDescending(a => a.ProfitLossPercent).Take(5).ToList(),
                WorstPerformers = assetDtos.OrderBy(a => a.ProfitLossPercent).Take(5).ToList(),
                RecentTransactions = recentTxns,
                Allocations = allocations
            };
        }
    }
}
