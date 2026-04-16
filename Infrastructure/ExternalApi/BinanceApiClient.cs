using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using CryptoVault.Application.DTOs;
using CryptoVault.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CryptoVault.Infrastructure.ExternalApi
{
    /// <summary>
    /// HttpClient-based implementation of IBinanceApiClient.
    /// Calls Binance public REST endpoints (no API key required).
    /// Includes caching, error handling, and rate limit awareness.
    /// </summary>
    public class BinanceApiClient : IBinanceApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly ILogger<BinanceApiClient> _logger;
        private readonly string _baseUrl;
        private readonly int _cacheTtlSeconds;

        // TradingView High-Definition SVG icon template
        private const string IconUrlTemplate = "https://s3-symbol-logo.tradingview.com/crypto/XTVC{0}.svg";

        // Cache of exchange symbols and logo mappings
        private List<BinanceSymbolInfo> _exchangeSymbols;
        private Dictionary<string, string> _logoMap = new();
        private DateTime _exchangeSymbolsLastFetch = DateTime.MinValue;
        private DateTime _logoMapLastFetch = DateTime.MinValue;

        public BinanceApiClient(HttpClient httpClient, IMemoryCache cache, IConfiguration config, ILogger<BinanceApiClient> logger)
        {
            _httpClient = httpClient;
            _cache = cache;
            _logger = logger;
            _baseUrl = config["Binance:BaseUrl"] ?? "https://api.binance.com";
            _cacheTtlSeconds = int.Parse(config["Binance:CacheTtlSeconds"] ?? "30");
        }

        public async Task<decimal> GetPriceAsync(string symbol)
        {
            var cacheKey = $"price_{symbol}";
            if (_cache.TryGetValue(cacheKey, out decimal cachedPrice))
                return cachedPrice;

            try
            {
                var response = await _httpClient.GetFromJsonAsync<BinancePriceResponse>(
                    $"{_baseUrl}/api/v3/ticker/price?symbol={symbol}");

                if (response != null && decimal.TryParse(response.Price, NumberStyles.Any, CultureInfo.InvariantCulture, out var price))
                {
                    _cache.Set(cacheKey, price, TimeSpan.FromSeconds(_cacheTtlSeconds));
                    return price;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch price for {Symbol} from Binance", symbol);
            }

            return 0m;
        }

        public async Task<Dictionary<string, decimal>> GetAllPricesAsync()
        {
            var cacheKey = "all_prices";
            if (_cache.TryGetValue(cacheKey, out Dictionary<string, decimal> cachedPrices))
                return cachedPrices;

            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<BinancePriceResponse>>(
                    $"{_baseUrl}/api/v3/ticker/price");

                if (response != null)
                {
                    var prices = new Dictionary<string, decimal>();
                    foreach (var item in response)
                    {
                        if (item.Symbol.EndsWith("USDT") &&
                            decimal.TryParse(item.Price, NumberStyles.Any, CultureInfo.InvariantCulture, out var price))
                        {
                            prices[item.Symbol] = price;
                        }
                    }

                    _cache.Set(cacheKey, prices, TimeSpan.FromSeconds(_cacheTtlSeconds));
                    return prices;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch all prices from Binance");
            }

            return new Dictionary<string, decimal>();
        }

        public async Task<MarketAssetDto> Get24hTickerAsync(string symbol)
        {
            var cacheKey = $"ticker24h_{symbol}";
            if (_cache.TryGetValue(cacheKey, out MarketAssetDto cachedTicker))
                return cachedTicker;

            try
            {
                var response = await _httpClient.GetFromJsonAsync<Binance24hTickerResponse>(
                    $"{_baseUrl}/api/v3/ticker/24hr?symbol={symbol}");

                if (response != null)
                {
                    var dto = MapTickerToDto(response);
                    _cache.Set(cacheKey, dto, TimeSpan.FromSeconds(_cacheTtlSeconds));
                    return dto;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch 24h ticker for {Symbol}", symbol);
            }

            return null;
        }

        public async Task<List<MarketAssetDto>> GetAll24hTickersAsync()
        {
            var cacheKey = "all_tickers_24h";
            if (_cache.TryGetValue(cacheKey, out List<MarketAssetDto> cachedTickers))
                return cachedTickers;

            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<Binance24hTickerResponse>>(
                    $"{_baseUrl}/api/v3/ticker/24hr");

                if (response != null)
                {
                    await EnsureLogosLoadedAsync();

                    var tickers = response
                        .Where(t => t.Symbol.EndsWith("USDT") && !t.Symbol.Contains("DOWN") && !t.Symbol.Contains("UP"))
                        .Select(MapTickerToDto)
                        .OrderByDescending(t => t.QuoteVolume)
                        .Take(200)
                        .ToList();

                    _cache.Set(cacheKey, tickers, TimeSpan.FromSeconds(_cacheTtlSeconds * 2));
                    return tickers;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch all 24h tickers from Binance");
            }

            return new List<MarketAssetDto>();
        }

        public async Task<List<MarketAssetDto>> SearchAssetsAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 1)
                return new List<MarketAssetDto>();

            var allTickers = await GetAll24hTickersAsync();
            var normalizedQuery = query.ToUpperInvariant();

            return allTickers
                .Where(t => t.Symbol.Contains(normalizedQuery) ||
                             t.BaseAsset.Contains(normalizedQuery) ||
                             t.DisplayName.ToUpperInvariant().Contains(normalizedQuery))
                .Take(20)
                .ToList();
        }

        public async Task<List<CandlestickDto>> GetKlinesAsync(string symbol, string interval = "1d", int limit = 30)
        {
            var cacheKey = $"klines_{symbol}_{interval}_{limit}";
            if (_cache.TryGetValue(cacheKey, out List<CandlestickDto> cachedKlines))
                return cachedKlines;

            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<JsonElement[]>>(
                    $"{_baseUrl}/api/v3/klines?symbol={symbol}&interval={interval}&limit={limit}");

                if (response != null)
                {
                    var klines = response.Select(k => new CandlestickDto
                    {
                        OpenTime = DateTimeOffset.FromUnixTimeMilliseconds(k[0].GetInt64()).UtcDateTime,
                        Open = ParseDecimal(k[1].GetString()),
                        High = ParseDecimal(k[2].GetString()),
                        Low = ParseDecimal(k[3].GetString()),
                        Close = ParseDecimal(k[4].GetString()),
                        Volume = ParseDecimal(k[5].GetString()),
                        CloseTime = DateTimeOffset.FromUnixTimeMilliseconds(k[6].GetInt64()).UtcDateTime
                    }).ToList();

                    _cache.Set(cacheKey, klines, TimeSpan.FromMinutes(5));
                    return klines;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch klines for {Symbol}", symbol);
            }

            return new List<CandlestickDto>();
        }

        // ====== Private Helpers ======

        private MarketAssetDto MapTickerToDto(Binance24hTickerResponse ticker)
        {
            var baseAsset = ticker.Symbol.Replace("USDT", "");
            
            // Use the TradingView SVG pattern requested by the user
            var imageUrl = string.Format(IconUrlTemplate, baseAsset.ToUpperInvariant());

            return new MarketAssetDto
            {
                Symbol = ticker.Symbol,
                BaseAsset = baseAsset,
                QuoteAsset = "USDT",
                DisplayName = GetDisplayName(baseAsset),
                ImageUrl = imageUrl,
                Price = ParseDecimal(ticker.LastPrice),
                PriceChangePercent = ParseDecimal(ticker.PriceChangePercent),
                PriceChange = ParseDecimal(ticker.PriceChange),
                HighPrice = ParseDecimal(ticker.HighPrice),
                LowPrice = ParseDecimal(ticker.LowPrice),
                Volume = ParseDecimal(ticker.Volume),
                QuoteVolume = ParseDecimal(ticker.QuoteVolume),
                IsActive = true
            };
        }

        private async Task EnsureLogosLoadedAsync()
        {
            if (_logoMap.Any() && DateTime.UtcNow - _logoMapLastFetch < TimeSpan.FromHours(6))
                return;

            try
            {
                // Internal Binance API to get all product info including logos
                var url = "https://www.binance.com/bapi/asset/v2/public/asset-service/product/get-products?includeEtf=true";
                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    var data = doc.RootElement.GetProperty("data");
                    
                    var newMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    foreach (var item in data.EnumerateArray())
                    {
                        var baseAsset = item.GetProperty("b").GetString();
                        var logoUrl = item.TryGetProperty("l", out var lProp) ? lProp.GetString() : null;
                        
                        if (!string.IsNullOrEmpty(baseAsset) && !string.IsNullOrEmpty(logoUrl))
                        {
                            newMap[baseAsset] = logoUrl;
                        }
                    }

                    if (newMap.Any())
                    {
                        _logoMap = newMap;
                        _logoMapLastFetch = DateTime.UtcNow;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load official Binance logo map");
            }
        }

        private static decimal ParseDecimal(string value)
        {
            if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
                return result;
            return 0m;
        }

        private static string GetDisplayName(string baseAsset)
        {
            // Common cryptocurrency names mapping
            var names = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"BTC", "Bitcoin"}, {"ETH", "Ethereum"}, {"BNB", "BNB"}, {"XRP", "Ripple"},
                {"ADA", "Cardano"}, {"SOL", "Solana"}, {"DOGE", "Dogecoin"}, {"DOT", "Polkadot"},
                {"AVAX", "Avalanche"}, {"MATIC", "Polygon"}, {"LINK", "Chainlink"}, {"UNI", "Uniswap"},
                {"ATOM", "Cosmos"}, {"LTC", "Litecoin"}, {"ETC", "Ethereum Classic"}, {"XLM", "Stellar"},
                {"ALGO", "Algorand"}, {"NEAR", "NEAR Protocol"}, {"FTM", "Fantom"}, {"AAVE", "Aave"},
                {"GRT", "The Graph"}, {"SAND", "The Sandbox"}, {"MANA", "Decentraland"}, {"AXS", "Axie Infinity"},
                {"SHIB", "Shiba Inu"}, {"CRO", "Cronos"}, {"TRX", "TRON"}, {"FIL", "Filecoin"},
                {"HBAR", "Hedera"}, {"APE", "ApeCoin"}, {"ICP", "Internet Computer"}, {"VET", "VeChain"},
                {"THETA", "Theta Network"}, {"ARB", "Arbitrum"}, {"OP", "Optimism"}, {"SUI", "Sui"},
                {"SEI", "Sei"}, {"TIA", "Celestia"}, {"JUP", "Jupiter"}, {"WIF", "dogwifhat"},
                {"PEPE", "Pepe"}, {"BONK", "Bonk"}, {"RENDER", "Render"}, {"INJ", "Injective"},
                {"IMX", "Immutable"}, {"APT", "Aptos"}, {"STX", "Stacks"}, {"MKR", "Maker"},
            };

            return names.TryGetValue(baseAsset, out var name) ? name : baseAsset;
        }
    }
}
