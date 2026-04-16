namespace CryptoVault.Application.DTOs
{
    /// <summary>
    /// Aggregated DTO for the main dashboard view.
    /// Contains all data needed to render the premium dashboard.
    /// </summary>
    public class PortfolioDashboardDto
    {
        public string PortfolioName { get; set; }
        public decimal InitialBudget { get; set; }
        public decimal AvailableBudget { get; set; }

        // ====== Portfolio Totals ======
        public decimal TotalInvested { get; set; }
        public decimal TotalPortfolioValue { get; set; }
        public decimal TotalProfitLoss => TotalPortfolioValue - TotalInvested;
        public decimal TotalProfitLossPercent => TotalInvested > 0 ? (TotalProfitLoss / TotalInvested) * 100m : 0m;

        /// <summary>
        /// Total account value = portfolio value + available budget
        /// </summary>
        public decimal TotalAccountValue => TotalPortfolioValue + AvailableBudget;

        // ====== Asset Details ======
        public int TotalAssets { get; set; }
        public int TotalTransactions { get; set; }
        public List<AssetDto> Assets { get; set; } = new();
        public List<AssetDto> TopPerformers { get; set; } = new();
        public List<AssetDto> WorstPerformers { get; set; } = new();
        public List<TransactionDto> RecentTransactions { get; set; } = new();

        // ====== Chart Data ======
        public List<AllocationChartItem> Allocations { get; set; } = new();
    }

    /// <summary>
    /// Item for portfolio allocation pie/donut chart.
    /// </summary>
    public class AllocationChartItem
    {
        public string Symbol { get; set; }
        public string Name { get; set; }
        public decimal Value { get; set; }
        public decimal Percentage { get; set; }
        public string Color { get; set; }
    }
}
