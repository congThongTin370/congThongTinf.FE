(function () {
    var source = document.getElementById("dashboard-chart-data");
    if (!source || typeof Chart === "undefined") {
        return;
    }

    var payload;
    try {
        payload = JSON.parse(source.textContent);
    } catch (e) {
        return;
    }

    var theme = getComputedStyle(document.body);
    var muted = (theme.getPropertyValue("--color-text-muted") || "#64748B").trim();
    var border = (theme.getPropertyValue("--color-border") || "#E2E8F0").trim();
    var navy = (theme.getPropertyValue("--color-navy") || "#102A43").trim();
    var gridLine = "#E2E8F0";

    Chart.defaults.font.family = "Inter, 'Segoe UI', Roboto, Arial, sans-serif";
    Chart.defaults.font.size = 12;
    Chart.defaults.color = muted;
    Chart.defaults.borderColor = border;
    Chart.defaults.plugins.legend.labels.boxWidth = 10;
    Chart.defaults.plugins.legend.labels.boxHeight = 10;
    Chart.defaults.plugins.legend.labels.usePointStyle = true;
    Chart.defaults.plugins.legend.labels.pointStyle = "rectRounded";
    Chart.defaults.animation.duration = 280;

    var grid = { color: gridLine };
    var ticks = { color: muted, maxRotation: 0, autoSkip: true, maxTicksLimit: 8, precision: 0 };

    function truncate(text, max) {
        if (!text) {
            return "";
        }
        return text.length > max ? text.slice(0, max - 1) + "…" : text;
    }

    function barOptions(horizontal) {
        return {
            indexAxis: horizontal ? "y" : "x",
            maintainAspectRatio: false,
            plugins: {
                legend: { display: !horizontal, position: "bottom" },
                tooltip: {
                    backgroundColor: navy,
                    titleColor: "#FFFFFF",
                    bodyColor: "#E0F2FE",
                    padding: 10,
                    cornerRadius: 6,
                    displayColors: true
                }
            },
            scales: {
                x: {
                    beginAtZero: true,
                    grid: horizontal ? grid : { display: false },
                    ticks: ticks,
                    border: { display: false }
                },
                y: {
                    beginAtZero: true,
                    grid: horizontal ? { display: false } : grid,
                    ticks: ticks,
                    border: { display: false }
                }
            }
        };
    }

    function renderBar(canvasId, config) {
        var canvas = document.getElementById(canvasId);
        if (!canvas || !config || !config.labels || config.labels.length === 0) {
            return;
        }

        var options = barOptions(!!config.horizontal);
        if (config.urls && config.urls.length) {
            options.onHover = function (event, elements) {
                var target = event.native && event.native.target;
                if (target) {
                    target.style.cursor = elements.length ? "pointer" : "default";
                }
            };
            options.onClick = function (_event, elements) {
                if (!elements.length) {
                    return;
                }
                var url = config.urls[elements[0].index];
                if (url) {
                    window.location.assign(url);
                }
            };
        }

        new Chart(canvas, {
            type: "bar",
            data: {
                labels: config.horizontal
                    ? config.labels.map(function (label) { return truncate(label, 28); })
                    : config.labels,
                datasets: config.datasets.map(function (set) {
                    return {
                        label: set.label,
                        data: set.data,
                        backgroundColor: set.color,
                        borderRadius: 3,
                        maxBarThickness: config.horizontal ? 18 : 22,
                        barPercentage: 0.72,
                        categoryPercentage: 0.7
                    };
                })
            },
            options: options
        });
    }

    function renderDoughnut(canvasId, config) {
        var canvas = document.getElementById(canvasId);
        if (!canvas || !config || !config.labels || config.labels.length === 0) {
            return;
        }

        new Chart(canvas, {
            type: "doughnut",
            data: {
                labels: config.labels,
                datasets: [{
                    data: config.data,
                    backgroundColor: config.colors,
                    borderWidth: 0,
                    hoverOffset: 4
                }]
            },
            options: {
                maintainAspectRatio: false,
                cutout: "68%",
                plugins: {
                    legend: { position: "bottom" },
                    tooltip: {
                        backgroundColor: navy,
                        padding: 10,
                        cornerRadius: 6
                    }
                }
            }
        });
    }

    renderBar("chart-news-by-day", payload.newsByDay);
    renderDoughnut("chart-news-status", payload.newsStatus);
    renderBar("chart-news-by-category", payload.newsByCategory);
    renderBar("chart-documents-by-category", payload.documentsByCategory);
    renderBar("chart-top-news", payload.topViewedNews);
})();
