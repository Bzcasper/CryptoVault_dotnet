using CryptoVault.Application.DTOs;
using CryptoVault.Application.Interfaces;
using CryptoVault.Infrastructure.ExternalApi;
using Microsoft.Extensions.Logging;

namespace CryptoVault.Application.Services
{
    /// <summary>
    /// Orchestrates market data retrieval by combining:
    /// 1. Real-time WebSocket prices (BinancePriceStreamService)
    /// 2. REST API fallback (IBinanceApiClient)
    /// 3. In-memory cache
    /// </summary>
    public class MarketDataService : IMarketDataService
    {
        private readonly IBinanceApiClient _apiClient;
        private readonly BinancePriceStreamService _priceStream;
        private readonly ILogger<MarketDataService> _logger;

        public MarketDataService(IBinanceApiClient apiClient, BinancePriceStreamService priceStream, ILogger<MarketDataService> logger)
        {
            _apiClient = apiClient;
            _priceStream = priceStream;
            _logger = logger;
        }

        public async Task<decimal> GetCurrentPriceAsync(string symbol)
        {
            // Priority 1: Real-time WebSocket price
            if (_priceStream.IsConnected && _priceStream.LatestPrices.TryGetValue(symbol, out var wsPrice))
            {
                return wsPrice;
            }

            // Priority 2: REST API with cache
            try
            {
                return await _apiClient.GetPriceAsync(symbol);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get price for {Symbol}", symbol);
                return 0m;
            }
        }

        public Dictionary<string, decimal> GetCachedPrices()
        {
            if (_priceStream.IsConnected && _priceStream.LatestPrices.Count > 0)
            {
                return new Dictionary<string, decimal>(_priceStream.LatestPrices);
            }

            return new Dictionary<string, decimal>();
        }

        public async Task<MarketAssetDto> GetMarketDataAsync(string symbol)
        {
            try
            {
                var ticker = await _apiClient.Get24hTickerAsync(symbol);

                // Enrich with WebSocket price if available
                if (ticker != null && _priceStream.IsConnected &&
                    _priceStream.LatestPrices.TryGetValue(symbol, out var wsPrice))
                {
                    ticker.Price = wsPrice;
                }

                return ticker;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get market data for {Symbol}", symbol);
                return null;
            }
        }

        public async Task<List<MarketAssetDto>> SearchAssetsAsync(string query)
        {
            return await _apiClient.SearchAssetsAsync(query);
        }

        public async Task<List<CandlestickDto>> GetChartDataAsync(string symbol, string interval = "1d", int limit = 30)
        {
            return await _apiClient.GetKlinesAsync(symbol, interval, limit);
        }

        public async Task<List<MarketAssetDto>> GetTopAssetsAsync(int count = 20)
        {
            var allTickers = await _apiClient.GetAll24hTickersAsync();
            return allTickers.Take(count).ToList();
        }

        public async Task<List<MarketAssetDto>> GetAllMarketAssetsAsync()
        {
            var allTickers = await _apiClient.GetAll24hTickersAsync();

            // Category mapping for well-known tokens
            var categories = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                // Layer 1
                {"BTC","Layer1"},{"ETH","Layer1"},{"SOL","Layer1"},{"ADA","Layer1"},{"AVAX","Layer1"},
                {"DOT","Layer1"},{"ATOM","Layer1"},{"NEAR","Layer1"},{"APT","Layer1"},{"SUI","Layer1"},
                {"SEI","Layer1"},{"TIA","Layer1"},{"FTM","Layer1"},{"ALGO","Layer1"},{"HBAR","Layer1"},
                {"XLM","Layer1"},{"ETC","Layer1"},{"ICP","Layer1"},{"VET","Layer1"},{"STX","Layer1"},

                // DeFi
                {"UNI","DeFi"},{"AAVE","DeFi"},{"MKR","DeFi"},{"LINK","DeFi"},{"GRT","DeFi"},
                {"INJ","DeFi"},{"JUP","DeFi"},{"CRV","DeFi"},{"SUSHI","DeFi"},{"COMP","DeFi"},
                {"SNX","DeFi"},{"LDO","DeFi"},{"DYDX","DeFi"},{"1INCH","DeFi"},{"CAKE","DeFi"},

                // Meme
                {"DOGE","Meme"},{"SHIB","Meme"},{"PEPE","Meme"},{"BONK","Meme"},{"WIF","Meme"},
                {"FLOKI","Meme"},{"TURBO","Meme"},{"NEIRO","Meme"},{"BOME","Meme"},

                // Metaverse / Gaming
                {"SAND","Metaverse"},{"MANA","Metaverse"},{"AXS","Metaverse"},{"ENJ","Metaverse"},
                {"GALA","Metaverse"},{"IMX","Metaverse"},{"THETA","Metaverse"},

                // AI
                {"RENDER","AI"},{"FET","AI"},{"AGIX","AI"},{"OCEAN","AI"},{"WLD","AI"},{"TAO","AI"},

                // Infrastructure
                {"BNB","Layer1"},{"XRP","Layer1"},{"TRX","Layer1"},{"LTC","Layer1"},{"FIL","Layer1"},
                {"ARB","Layer2"},{"OP","Layer2"},{"MATIC","Layer2"},
            };

            for (int i = 0; i < allTickers.Count; i++)
            {
                allTickers[i].Rank = i + 1;
                if (categories.TryGetValue(allTickers[i].BaseAsset, out var cat))
                {
                    allTickers[i].Category = cat;
                }
            }

            return allTickers;
        }
    }
}
