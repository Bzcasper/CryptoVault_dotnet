using CryptoVault.Components;
using CryptoVault.Application.Interfaces;
using CryptoVault.Application.Services;
using CryptoVault.Infrastructure.Data;
using CryptoVault.Infrastructure.ExternalApi;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ====== Blazor Server Components ======
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ====== Entity Framework Core with SQLite ======
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

// ====== In-Memory Cache ======
builder.Services.AddMemoryCache();

// ====== HttpClient for Binance API ======
builder.Services.AddHttpClient<IBinanceApiClient, BinanceApiClient>(client =>
{
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
    client.Timeout = TimeSpan.FromSeconds(15);
});

// ====== Background Services ======
builder.Services.AddSingleton<BinancePriceStreamService>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<BinancePriceStreamService>());

// ====== Application Services (Scoped — per-request lifecycle) ======
builder.Services.AddScoped<IPortfolioService, PortfolioService>();
builder.Services.AddScoped<IAssetService, AssetService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IWatchlistService, WatchlistService>();
builder.Services.AddScoped<IMarketDataService, MarketDataService>();
builder.Services.AddScoped<IAlertService, AlertService>();

var app = builder.Build();

// ====== Database Initialization ======
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Ensure database is created (applies pending migrations or creates tables)
    dbContext.Database.EnsureCreated();

    Console.WriteLine("[OK] Database initialized successfully.");
}

// ====== HTTP Pipeline Configuration ======
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

Console.WriteLine("[CryptoVault] Starting application...");
Console.WriteLine("[CryptoVault] Binance API: Real-time cryptocurrency data");
Console.WriteLine("[CryptoVault] Initial budget: $" + builder.Configuration["Portfolio:InitialBudget"]);

app.Run();
