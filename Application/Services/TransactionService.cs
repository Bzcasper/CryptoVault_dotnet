using CryptoVault.Application.DTOs;
using CryptoVault.Application.Interfaces;
using CryptoVault.Domain.Entities;
using CryptoVault.Domain.Enums;
using CryptoVault.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CryptoVault.Application.Services
{
    /// <summary>
    /// Implements buy/sell trade execution with budget validation,
    /// fee calculation (0.1%), and full audit trail.
    /// </summary>
    public class TransactionService : ITransactionService
    {
        private readonly AppDbContext _db;
        private readonly IAssetService _assetService;
        private readonly ILogger<TransactionService> _logger;
        private const decimal FeeRate = 0.001m; // 0.1% fee like Binance

        public TransactionService(AppDbContext db, IAssetService assetService, ILogger<TransactionService> logger)
        {
            _db = db;
            _assetService = assetService;
            _logger = logger;
        }

        public async Task<TransactionDto> ExecuteTradeAsync(
            int portfolioId, string symbol, string name, string imageUrl,
            TransactionType type, decimal quantity, decimal pricePerUnit)
        {
            var portfolio = await _db.Portfolios.FindAsync(portfolioId);
            if (portfolio == null)
                throw new InvalidOperationException("Portfolio not found.");

            var totalAmount = quantity * pricePerUnit;
            var fee = totalAmount * FeeRate;
            var totalCost = totalAmount + fee;

            if (type == TransactionType.Buy)
            {
                // Validate budget
                if (totalCost > portfolio.CurrentBudget)
                    throw new InvalidOperationException(
                        $"Insufficient budget. Required: ${totalCost:N2}, Available: ${portfolio.CurrentBudget:N2}");

                // Deduct from budget
                portfolio.CurrentBudget -= totalCost;

                // Add/update asset holding
                var asset = await _assetService.AddOrUpdateAssetAsync(
                    portfolioId, symbol, name, imageUrl, quantity, pricePerUnit);

                // Record transaction
                var transaction = new Transaction
                {
                    Symbol = symbol,
                    AssetName = name,
                    Type = TransactionType.Buy,
                    Quantity = quantity,
                    PricePerUnit = pricePerUnit,
                    TotalAmount = totalAmount,
                    Fee = fee,
                    PortfolioId = portfolioId,
                    AssetId = asset.Id,
                    ExecutedAt = DateTime.UtcNow
                };

                _db.Transactions.Add(transaction);
                portfolio.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();

                _logger.LogInformation("BUY {Qty} {Symbol} @ ${Price} = ${Total} (fee: ${Fee})",
                    quantity, symbol, pricePerUnit, totalAmount, fee);

                return MapToDto(transaction);
            }
            else // Sell
            {
                var asset = await _assetService.GetAssetBySymbolAsync(portfolioId, symbol);
                if (asset == null || asset.Quantity < quantity)
                    throw new InvalidOperationException(
                        $"Insufficient holdings. Available: {asset?.Quantity ?? 0} {symbol}");

                var revenue = totalAmount - fee;

                // Add to budget
                portfolio.CurrentBudget += revenue;

                // Reduce asset holding
                await _assetService.ReduceAssetAsync(asset.Id, quantity);

                // Record transaction
                var transaction = new Transaction
                {
                    Symbol = symbol,
                    AssetName = name,
                    Type = TransactionType.Sell,
                    Quantity = quantity,
                    PricePerUnit = pricePerUnit,
                    TotalAmount = totalAmount,
                    Fee = fee,
                    PortfolioId = portfolioId,
                    AssetId = asset.Quantity > quantity ? asset.Id : null,
                    ExecutedAt = DateTime.UtcNow
                };

                _db.Transactions.Add(transaction);
                portfolio.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();

                _logger.LogInformation("SELL {Qty} {Symbol} @ ${Price} = ${Total} (fee: ${Fee})",
                    quantity, symbol, pricePerUnit, totalAmount, fee);

                return MapToDto(transaction);
            }
        }

        public async Task<List<TransactionDto>> GetTransactionsAsync(
            int portfolioId, string symbolFilter = null, TransactionType? typeFilter = null, int? limit = null)
        {
            var query = _db.Transactions
                .Where(t => t.PortfolioId == portfolioId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(symbolFilter))
                query = query.Where(t => t.Symbol.Contains(symbolFilter));

            if (typeFilter.HasValue)
                query = query.Where(t => t.Type == typeFilter.Value);

            query = query.OrderByDescending(t => t.ExecutedAt);

            if (limit.HasValue)
                query = query.Take(limit.Value);

            return await query.Select(t => MapToDto(t)).ToListAsync();
        }

        public async Task<List<TransactionDto>> GetRecentTransactionsAsync(int portfolioId, int count = 10)
        {
            return await GetTransactionsAsync(portfolioId, limit: count);
        }

        public async Task<int> GetTransactionCountAsync(int portfolioId)
        {
            return await _db.Transactions.CountAsync(t => t.PortfolioId == portfolioId);
        }

        private static TransactionDto MapToDto(Transaction t)
        {
            return new TransactionDto
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
            };
        }
    }
}
