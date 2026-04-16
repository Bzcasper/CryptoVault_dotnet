using System.Collections.Concurrent;
using System.Globalization;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CryptoVault.Infrastructure.ExternalApi
{
    /// <summary>
    /// Background service that maintains a WebSocket connection to Binance
    /// for real-time price streaming. Stores latest prices in a thread-safe
    /// concurrent dictionary accessible via DI.
    /// </summary>
    public class BinancePriceStreamService : BackgroundService
    {
        private readonly ILogger<BinancePriceStreamService> _logger;
        private readonly string _wsUrl;

        /// <summary>
        /// Thread-safe dictionary of latest prices. Key = Symbol (e.g., "BTCUSDT").
        /// Accessible by any service via DI.
        /// </summary>
        public ConcurrentDictionary<string, decimal> LatestPrices { get; } = new();

        /// <summary>
        /// Last time a price update was received from the WebSocket.
        /// </summary>
        public DateTime LastUpdateTime { get; private set; } = DateTime.MinValue;

        /// <summary>
        /// Whether the WebSocket is currently connected.
        /// </summary>
        public bool IsConnected { get; private set; }

        public BinancePriceStreamService(IConfiguration config, ILogger<BinancePriceStreamService> logger)
        {
            _logger = logger;
            _wsUrl = config["Binance:WebSocketUrl"] ?? "wss://stream.binance.com:9443/ws";
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("BinancePriceStreamService starting...");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ConnectAndStreamAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    IsConnected = false;
                    _logger.LogWarning(ex, "WebSocket connection lost. Reconnecting in 5 seconds...");
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }

            _logger.LogInformation("BinancePriceStreamService stopped.");
        }

        private async Task ConnectAndStreamAsync(CancellationToken cancellationToken)
        {
            using var ws = new ClientWebSocket();
            ws.Options.KeepAliveInterval = TimeSpan.FromSeconds(20);

            // Connect to the all mini tickers stream (covers all symbols)
            var streamUrl = $"{_wsUrl}/!miniTicker@arr";
            _logger.LogInformation("Connecting to Binance WebSocket: {Url}", streamUrl);

            await ws.ConnectAsync(new Uri(streamUrl), cancellationToken);
            IsConnected = true;
            _logger.LogInformation("Connected to Binance WebSocket stream.");

            var buffer = new byte[8192];

            while (ws.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
            {
                var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    _logger.LogInformation("WebSocket closed by server.");
                    break;
                }

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var json = Encoding.UTF8.GetString(buffer, 0, result.Count);

                    // Handle fragmented messages
                    if (!result.EndOfMessage)
                    {
                        var fullMessage = new StringBuilder(json);
                        while (!result.EndOfMessage)
                        {
                            result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                            fullMessage.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
                        }
                        json = fullMessage.ToString();
                    }

                    ProcessMiniTickerArray(json);
                }
            }

            IsConnected = false;
        }

        private void ProcessMiniTickerArray(string json)
        {
            try
            {
                var tickers = JsonSerializer.Deserialize<List<BinanceMiniTickerStream>>(json);
                if (tickers == null) return;

                foreach (var ticker in tickers)
                {
                    if (ticker.Symbol != null && ticker.Symbol.EndsWith("USDT"))
                    {
                        if (decimal.TryParse(ticker.ClosePrice, NumberStyles.Any, CultureInfo.InvariantCulture, out var price))
                        {
                            LatestPrices[ticker.Symbol] = price;
                        }
                    }
                }

                LastUpdateTime = DateTime.UtcNow;
            }
            catch (JsonException ex)
            {
                _logger.LogDebug(ex, "Failed to parse WebSocket message");
            }
        }
    }
}
