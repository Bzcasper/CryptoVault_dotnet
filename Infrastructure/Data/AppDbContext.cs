using Microsoft.EntityFrameworkCore;
using CryptoVault.Domain.Entities;

namespace CryptoVault.Infrastructure.Data
{
    /// <summary>
    /// Entity Framework Core database context for CryptoVault.
    /// Manages all entity-to-table mappings and relationships.
    /// </summary>
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // ====== DbSets — Each represents a table in the SQLite database ======

        public DbSet<Portfolio> Portfolios { get; set; }
        public DbSet<Asset> Assets { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<WatchlistItem> WatchlistItems { get; set; }
        public DbSet<PriceSnapshot> PriceSnapshots { get; set; }
        public DbSet<Alert> Alerts { get; set; }

        /// <summary>
        /// Configure entity relationships and constraints using Fluent API.
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ====== Portfolio Configuration ======
            modelBuilder.Entity<Portfolio>(entity =>
            {
                entity.HasIndex(p => p.Name).IsUnique();
                entity.Property(p => p.InitialBudget).HasColumnType("decimal(18,2)");
                entity.Property(p => p.CurrentBudget).HasColumnType("decimal(18,2)");
            });

            // ====== Asset Configuration ======
            modelBuilder.Entity<Asset>(entity =>
            {
                // Composite unique: One symbol per portfolio
                entity.HasIndex(a => new { a.PortfolioId, a.Symbol }).IsUnique();
                entity.Property(a => a.Quantity).HasColumnType("decimal(18,8)");
                entity.Property(a => a.AverageBuyPrice).HasColumnType("decimal(18,8)");
                entity.Property(a => a.TotalInvested).HasColumnType("decimal(18,2)");

                // Relationship: Asset belongs to Portfolio
                entity.HasOne(a => a.Portfolio)
                      .WithMany(p => p.Assets)
                      .HasForeignKey(a => a.PortfolioId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ====== Transaction Configuration ======
            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.Property(t => t.Quantity).HasColumnType("decimal(18,8)");
                entity.Property(t => t.PricePerUnit).HasColumnType("decimal(18,8)");
                entity.Property(t => t.TotalAmount).HasColumnType("decimal(18,2)");
                entity.Property(t => t.Fee).HasColumnType("decimal(18,4)");

                // Relationship: Transaction belongs to Portfolio
                entity.HasOne(t => t.Portfolio)
                      .WithMany(p => p.Transactions)
                      .HasForeignKey(t => t.PortfolioId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Relationship: Transaction optionally belongs to Asset
                entity.HasOne(t => t.Asset)
                      .WithMany(a => a.Transactions)
                      .HasForeignKey(t => t.AssetId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(t => t.ExecutedAt);
                entity.HasIndex(t => t.Symbol);
            });

            // ====== WatchlistItem Configuration ======
            modelBuilder.Entity<WatchlistItem>(entity =>
            {
                entity.HasIndex(w => new { w.PortfolioId, w.Symbol }).IsUnique();

                entity.HasOne(w => w.Portfolio)
                      .WithMany(p => p.WatchlistItems)
                      .HasForeignKey(w => w.PortfolioId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ====== PriceSnapshot Configuration ======
            modelBuilder.Entity<PriceSnapshot>(entity =>
            {
                entity.Property(ps => ps.Price).HasColumnType("decimal(18,8)");
                entity.Property(ps => ps.Volume24h).HasColumnType("decimal(18,2)");
                entity.Property(ps => ps.ChangePercent24h).HasColumnType("decimal(8,4)");

                entity.HasIndex(ps => new { ps.Symbol, ps.CapturedAt });
            });

            // ====== Alert Configuration ======
            modelBuilder.Entity<Alert>(entity =>
            {
                entity.Property(a => a.TargetPrice).HasColumnType("decimal(18,8)");

                entity.HasOne(a => a.Portfolio)
                      .WithMany(p => p.Alerts)
                      .HasForeignKey(a => a.PortfolioId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(a => new { a.Symbol, a.IsActive });
            });
        }
    }
}
