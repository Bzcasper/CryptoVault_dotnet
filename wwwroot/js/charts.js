// ================================================================
// CryptoVault — Chart.js Integration Helper
// Provides functions for creating premium dark-themed charts
// ================================================================

// Global chart defaults for dark theme
if (typeof Chart !== 'undefined') {
    Chart.defaults.color = '#848E9C';
    Chart.defaults.borderColor = 'rgba(234, 236, 239, 0.08)';
    Chart.defaults.font.family = "'Inter', sans-serif";
}

// Store chart instances for cleanup
window.cvCharts = {};

/**
 * Creates or updates a donut chart for portfolio allocation.
 */
window.createAllocationChart = function (canvasId, labels, values, colors) {
    const canvas = document.getElementById(canvasId);
    if (!canvas) return;

    // Destroy existing chart
    if (window.cvCharts[canvasId]) {
        window.cvCharts[canvasId].destroy();
    }

    const ctx = canvas.getContext('2d');
    window.cvCharts[canvasId] = new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: labels,
            datasets: [{
                data: values,
                backgroundColor: colors,
                borderColor: '#1E2329',
                borderWidth: 2,
                hoverOffset: 6,
                borderRadius: 4
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            cutout: '72%',
            plugins: {
                legend: {
                    display: true,
                    position: 'right',
                    labels: {
                        color: '#EAECEF',
                        font: { size: 11, weight: '500' },
                        padding: 12,
                        usePointStyle: true,
                        pointStyleWidth: 10,
                        generateLabels: function(chart) {
                            const data = chart.data;
                            return data.labels.map((label, i) => ({
                                text: `${label}  ${data.datasets[0].data[i].toFixed(1)}%`,
                                fillStyle: data.datasets[0].backgroundColor[i],
                                strokeStyle: 'transparent',
                                pointStyle: 'rectRounded',
                                fontColor: '#EAECEF'
                            }));
                        }
                    }
                },
                tooltip: {
                    backgroundColor: '#1E2329',
                    titleColor: '#848E9C',
                    bodyColor: '#EAECEF',
                    borderColor: 'rgba(234, 236, 239, 0.08)',
                    borderWidth: 1,
                    padding: 10,
                    displayColors: false,
                    callbacks: {
                        label: function(context) { return `${context.label}: ${context.raw}%`; }
                    }
                }
            }
        }
    });
};

window.updateAllocationChart = function (canvasId, labels, values, colors) {
    if (window.cvCharts && window.cvCharts[canvasId]) {
        window.cvCharts[canvasId].data.labels = labels;
        window.cvCharts[canvasId].data.datasets[0].data = values;
        window.cvCharts[canvasId].data.datasets[0].backgroundColor = colors;
        window.cvCharts[canvasId].update();
    } else {
        window.createAllocationChart(canvasId, labels, values, colors);
    }
};

/**
 * Creates or updates a line chart for portfolio value over time.
 */
window.createLineChart = function (canvasId, labels, values, color) {
    const canvas = document.getElementById(canvasId);
    if (!canvas) return;

    if (window.cvCharts[canvasId]) {
        window.cvCharts[canvasId].destroy();
    }

    color = color || '#F0B90B';
    const ctx = canvas.getContext('2d');

    // Create gradient fill
    const gradient = ctx.createLinearGradient(0, 0, 0, canvas.height);
    gradient.addColorStop(0, color + '30');
    gradient.addColorStop(1, color + '05');

    window.cvCharts[canvasId] = new Chart(ctx, {
        type: 'line',
        data: {
            labels: labels,
            datasets: [{
                data: values,
                borderColor: color,
                backgroundColor: gradient,
                borderWidth: 2,
                fill: true,
                tension: 0.4,
                pointRadius: 0,
                pointHoverRadius: 5,
                pointHoverBackgroundColor: color,
                pointHoverBorderColor: '#1E2329',
                pointHoverBorderWidth: 2
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            interaction: {
                intersect: false,
                mode: 'index'
            },
            plugins: {
                legend: { display: false },
                tooltip: {
                    backgroundColor: '#2B3139',
                    titleColor: '#EAECEF',
                    bodyColor: '#EAECEF',
                    borderColor: 'rgba(234,236,239,0.15)',
                    borderWidth: 1,
                    cornerRadius: 8,
                    padding: 10,
                    callbacks: {
                        label: function(ctx) {
                            return `$${ctx.parsed.y.toLocaleString('en-US', { minimumFractionDigits: 2 })}`;
                        }
                    }
                }
            },
            scales: {
                x: {
                    grid: { display: false },
                    ticks: {
                        color: '#5E6673',
                        font: { size: 10 },
                        maxTicksLimit: 8
                    }
                },
                y: {
                    grid: {
                        color: 'rgba(234, 236, 239, 0.05)',
                        drawBorder: false
                    },
                    ticks: {
                        color: '#5E6673',
                        font: { size: 10 },
                        callback: function(value) {
                            return '$' + value.toLocaleString();
                        }
                    }
                }
            }
        }
    });
};

/**
 * Creates or updates a bar chart for asset performance comparison.
 */
window.createBarChart = function (canvasId, labels, values, colors) {
    const canvas = document.getElementById(canvasId);
    if (!canvas) return;

    if (window.cvCharts[canvasId]) {
        window.cvCharts[canvasId].destroy();
    }

    // If no colors provided, use green for positive, red for negative
    if (!colors) {
        colors = values.map(v => v >= 0 ? '#0ECB81' : '#F6465D');
    }

    const ctx = canvas.getContext('2d');
    window.cvCharts[canvasId] = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [{
                data: values,
                backgroundColor: colors.map(c => c + 'CC'),
                borderColor: colors,
                borderWidth: 1,
                borderRadius: 6,
                borderSkipped: false
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: { display: false },
                tooltip: {
                    backgroundColor: '#2B3139',
                    titleColor: '#EAECEF',
                    bodyColor: '#EAECEF',
                    borderColor: 'rgba(234,236,239,0.15)',
                    borderWidth: 1,
                    cornerRadius: 8,
                    padding: 10,
                    callbacks: {
                        label: function(ctx) {
                            return `P&L: ${ctx.parsed.y >= 0 ? '+' : ''}${ctx.parsed.y.toFixed(2)}%`;
                        }
                    }
                }
            },
            scales: {
                x: {
                    grid: { display: false },
                    ticks: { color: '#848E9C', font: { size: 11, weight: '500' } }
                },
                y: {
                    grid: {
                        color: 'rgba(234, 236, 239, 0.05)',
                        drawBorder: false
                    },
                    ticks: {
                        color: '#5E6673',
                        font: { size: 10 },
                        callback: function(value) {
                            return value + '%';
                        }
                    }
                }
            }
        }
    });
};

/**
 * Destroys a specific chart instance.
 */
window.destroyChart = function (canvasId) {
    if (window.cvCharts[canvasId]) {
        window.cvCharts[canvasId].destroy();
        delete window.cvCharts[canvasId];
    }
};

/**
 * Creates or updates a dual-axis price chart with volume bars.
 * Used on the Trade page.
 */
window.createPriceChart = function (containerId, candleData, volumeData) {
    const container = document.getElementById(containerId);
    if (!container) return;

    if (window.cvCharts && window.cvCharts[containerId]) {
        // Disconnect observers to prevent memory leaks throwing disposed exceptions
        if (window.cvCharts[containerId].resizeObserver) {
            window.cvCharts[containerId].resizeObserver.disconnect();
        }

        // Destroy existing TV chart or canvas fallback
        if (typeof window.cvCharts[containerId].remove === 'function') {
            window.cvCharts[containerId].remove();
        } else if (typeof window.cvCharts[containerId].destroy === 'function') {
            window.cvCharts[containerId].destroy();
        }
        container.innerHTML = '';
    } else {
        window.cvCharts = window.cvCharts || {};
    }

    if (typeof LightweightCharts === 'undefined') {
        console.error("LightweightCharts is not loaded yet.");
        return;
    }

    // Set height exactly to parent
    const chartHeight = container.clientHeight || 442;
    const chartWidth = container.clientWidth || window.innerWidth * 0.7;

    const chart = LightweightCharts.createChart(container, {
        width: chartWidth,
        height: chartHeight,
        layout: {
            background: { type: 'solid', color: 'transparent' },
            textColor: '#848E9C',
            fontFamily: 'BinanceNova, Arial, sans-serif'
        },
        grid: {
            vertLines: { color: 'rgba(234, 236, 239, 0.05)' },
            horzLines: { color: 'rgba(234, 236, 239, 0.05)' },
        },
        timeScale: {
            timeVisible: true,
            secondsVisible: false,
            borderColor: '#333B47'
        },
        rightPriceScale: {
            borderColor: '#333B47',
            autoScale: true
        }
    });

    // Add main candlestick series
    const candleSeries = chart.addCandlestickSeries({
        upColor: '#0ECB81',
        downColor: '#F6465D',
        borderDownColor: '#F6465D',
        borderUpColor: '#0ECB81',
        wickDownColor: '#F6465D',
        wickUpColor: '#0ECB81',
        priceFormat: {
            type: 'price',
            precision: 5,
            minMove: 0.00001
        }
    });
    candleSeries.setData(candleData);

    // Add volume histogram at the bottom
    const volumeSeries = chart.addHistogramSeries({
        color: '#26a69a',
        priceFormat: {
            type: 'volume',
        },
        priceScaleId: '', // set as an overlay by default
    });

    chart.priceScale('').applyOptions({
        scaleMargins: {
            top: 0.8, // highest point of the series will be at 80% from the top
            bottom: 0,
        },
    });
    
    volumeSeries.setData(volumeData);

    window.cvCharts[containerId] = chart;
    window.cvCharts[containerId].candleSeries = candleSeries;

    // Handle resize locally relative to container
    const resizeObserver = new ResizeObserver(entries => {
        if (entries.length === 0 || entries[0].target !== container) { return; }
        const newRect = entries[0].contentRect;
        chart.applyOptions({ height: newRect.height, width: newRect.width });
    });
    resizeObserver.observe(container);
    window.cvCharts[containerId].resizeObserver = resizeObserver;
};

window.updatePriceChartTick = function(containerId, tick) {
    if (window.cvCharts && window.cvCharts[containerId] && window.cvCharts[containerId].candleSeries) {
        window.cvCharts[containerId].candleSeries.update(tick);
    }
};

/**
 * Creates a tiny sparkline chart for market table rows.
 * No axes, no labels, just a colored line.
 */
window.createSparkline = function (canvasId, values, color) {
    const canvas = document.getElementById(canvasId);
    if (!canvas) return;

    if (window.cvCharts[canvasId]) {
        window.cvCharts[canvasId].destroy();
    }

    color = color || '#F0B90B';
    const ctx = canvas.getContext('2d');

    const gradient = ctx.createLinearGradient(0, 0, 0, canvas.height);
    gradient.addColorStop(0, color + '40');
    gradient.addColorStop(1, color + '00');

    window.cvCharts[canvasId] = new Chart(ctx, {
        type: 'line',
        data: {
            labels: values.map((_, i) => i),
            datasets: [{
                data: values,
                borderColor: color,
                backgroundColor: gradient,
                borderWidth: 1.5,
                fill: true,
                tension: 0.4,
                pointRadius: 0,
                pointHoverRadius: 0
            }]
        },
        options: {
            responsive: false,
            maintainAspectRatio: false,
            animation: { duration: 0 },
            plugins: {
                legend: { display: false },
                tooltip: { enabled: false }
            },
            scales: {
                x: { display: false },
                y: { display: false }
            }
        }
    });
};
