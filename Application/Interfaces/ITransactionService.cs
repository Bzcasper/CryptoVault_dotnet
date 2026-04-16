using CryptoVault.Application.DTOs;
using CryptoVault.Domain.Enums;

namespace CryptoVault.Application.Interfaces
{
    /// <summary>
    /// Service interface for transaction processing.
    /// Handles buy/sell operations with budget validation.
    /// </summary>
    public interface ITransactionService
    {
        Task<TransactionDto> ExecuteTradeAsync(int portfolioId, string symbol, string name, string imageUrl, TransactionType type, decimal quantity, decimal pricePerUnit);
        Task<List<TransactionDto>> GetTransactionsAsync(int portfolioId, string symbolFilter = null, TransactionType? typeFilter = null, int? limit = null);
        Task<List<TransactionDto>> GetRecentTransactionsAsync(int portfolioId, int count = 10);
        Task<int> GetTransactionCountAsync(int portfolioId);
    }
}
