using System.Collections.ObjectModel;
using System.Text.Json;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.UI;
using CSC13001_my_shop_project.Services;

namespace CSC13001_my_shop_project.Presentation.Dashboard;

public partial class DashboardViewModel : ObservableObject
{
    private readonly OrderService? _orderService;
    private readonly GraphqlService? _graphql;

    [ObservableProperty]
    private string totalProducts = "—";

    [ObservableProperty]
    private string totalOrders = "—";

    [ObservableProperty]
    private string totalRevenue = "—";

    [ObservableProperty]
    private ObservableCollection<DashboardOrderItem> _recentOrders = new();

    [ObservableProperty]
    private bool _isLoadingOrders;

    [ObservableProperty]
    private ObservableCollection<LowStockItem> _lowStockItems = new();

    [ObservableProperty]
    private ObservableCollection<BestSellingItem> _bestSellingItems = new();

    [ObservableProperty]
    private string _lowStockBadgeText = "0 items";

    // ── Revenue chart data ─────────────────────────────────────────────
    [ObservableProperty]
    private ObservableCollection<RevenueChartPoint> _revenueChartPoints = new();

    [ObservableProperty]
    private string _chartSubtitle = "Loading chart data…";

    [ObservableProperty]
    private string _chartPeriodLabel = "Daily";

    /// <summary>
    /// Raised after chart data has been loaded and is ready to be drawn.
    /// </summary>
    public event Action? ChartDataReady;

    /// <summary>
    /// Parameterless constructor for design-time / fallback.
    /// </summary>
    public DashboardViewModel()
    {
    }

    /// <summary>
    /// Constructor with DI — receives OrderService and GraphqlService to fetch real data.
    /// </summary>
    public DashboardViewModel(OrderService orderService, GraphqlService graphqlService) : this()
    {
        _orderService = orderService;
        _graphql = graphqlService;
        _ = LoadAllDashboardDataAsync();
    }

    /// <summary>
    /// Loads all dashboard data in parallel.
    /// </summary>
    public async Task LoadAllDashboardDataAsync()
    {
        await Task.WhenAll(
            LoadRecentOrdersAsync(),
            LoadTopLowStockProductsAsync(),
            LoadTopSellingProductsAsync(),
            LoadTotalProductsAsync(),
            LoadSalesOverviewAsync(),
            LoadRevenueChartAsync()
        );
    }

    /// <summary>
    /// Fetches sales overview from backend Report API → populates TotalOrders and TotalRevenue stat cards.
    /// </summary>
    private async Task LoadSalesOverviewAsync()
    {
        if (_graphql is null) return;

        try
        {
            var today = DateTime.Today.ToString("yyyy-MM-dd");

            var data = await _graphql.QueryAsync(
                @"query SalesOverview($startDate: String, $endDate: String) {
                    salesOverview(startDate: $startDate, endDate: $endDate) {
                        totalOrders
                        totalRevenue
                        totalItemsSold
                        uniqueCustomers
                        avgOrderValue
                    }
                }",
                new { startDate = today, endDate = today }
            );

            var overview = data.GetProperty("salesOverview");

            var orders = overview.GetProperty("totalOrders").GetInt32();
            var revenue = overview.GetProperty("totalRevenue").GetInt64();

            App.RunOnUIThread(() =>
            {
                TotalOrders = orders.ToString("N0");
                TotalRevenue = $"{revenue:N0} ₫";
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Dashboard] Failed to load sales overview: {ex.Message}");
        }
    }

    /// <summary>
    /// Fetches daily revenue report from backend → populates chart data points.
    /// Uses the last 30 days as the default date range.
    /// Falls back to mock data if API returns no results.
    /// </summary>
    private async Task LoadRevenueChartAsync()
    {
        try
        {
            var endDate = DateTime.Today.ToString("yyyy-MM-dd");
            var startDate = DateTime.Today.AddDays(-29).ToString("yyyy-MM-dd");

            List<RevenueChartPoint> chartPoints = new();

            if (_graphql is not null)
            {
                try
                {
                    var data = await _graphql.QueryAsync(
                        @"query RevenueReport($period: String, $startDate: String, $endDate: String) {
                            revenueReport(period: $period, startDate: $startDate, endDate: $endDate) {
                                period
                                date
                                totalOrders
                                totalRevenue
                                totalItemsSold
                                avgOrderValue
                            }
                        }",
                        new { period = "day", startDate, endDate }
                    );

                    var periods = data.GetProperty("revenueReport").EnumerateArray();

                    foreach (var p in periods)
                    {
                        var dateStr = p.GetProperty("date").GetString() ?? "";
                        var revenue = p.GetProperty("totalRevenue").GetInt64();
                        var ordersCount = p.GetProperty("totalOrders").GetInt32();

                        var label = dateStr;
                        if (DateTime.TryParse(dateStr, out var dt))
                        {
                            label = dt.ToString("dd/MM");
                        }

                        chartPoints.Add(new RevenueChartPoint(label, revenue, ordersCount, dateStr));
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[Dashboard] API failed, using mock chart data: {ex.Message}");
                }
            }

            // Fallback: generate mock data if API returned nothing
            if (chartPoints.Count == 0)
            {
                chartPoints = GenerateMockChartData();
            }

            // Sort by date ascending for chart rendering
            chartPoints.Sort((a, b) => string.Compare(a.RawDate, b.RawDate, StringComparison.Ordinal));

            // Compute subtitle
            var from = DateTime.TryParse(startDate, out var s) ? s.ToString("dd/MM/yyyy") : startDate;
            var to = DateTime.TryParse(endDate, out var e) ? e.ToString("dd/MM/yyyy") : endDate;
            var subtitle = $"Doanh thu theo ngày — {from} → {to}";

            // Assign on UI thread
            App.RunOnUIThread(() =>
            {
                RevenueChartPoints = new ObservableCollection<RevenueChartPoint>(chartPoints);
                ChartSubtitle = subtitle;
            });

            // Notify code-behind to redraw the chart
            ChartDataReady?.Invoke();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Dashboard] Failed to load revenue chart: {ex.Message}");

            // Even on total failure, show mock data so chart isn't blank
            var mockData = GenerateMockChartData();
            App.RunOnUIThread(() =>
            {
                RevenueChartPoints = new ObservableCollection<RevenueChartPoint>(mockData);
                ChartSubtitle = "Dữ liệu mẫu — không kết nối được server";
            });
            ChartDataReady?.Invoke();
        }
    }

    /// <summary>
    /// Generates 30 days of realistic mock revenue data for chart display.
    /// </summary>
    private static List<RevenueChartPoint> GenerateMockChartData()
    {
        var points = new List<RevenueChartPoint>();
        var rng = new Random(42); // Fixed seed for consistent display
        var baseDate = DateTime.Today.AddDays(-29);

        for (int i = 0; i < 30; i++)
        {
            var date = baseDate.AddDays(i);
            // Generate realistic-looking revenue: 500K → 5M range with some variation
            var baseRevenue = 1_500_000L + (long)(Math.Sin(i * 0.5) * 800_000);
            var revenue = baseRevenue + rng.Next(-300_000, 500_000);
            if (revenue < 200_000) revenue = 200_000 + rng.Next(100_000);
            var orders = (int)(revenue / 350_000) + rng.Next(1, 5);

            points.Add(new RevenueChartPoint(
                date.ToString("dd/MM"),
                revenue,
                orders,
                date.ToString("yyyy-MM-dd")
            ));
        }

        return points;
    }

    /// <summary>
    /// Fetches top 3 most recent orders from the backend.
    /// </summary>
    public async Task LoadRecentOrdersAsync()
    {
        if (_orderService is null) return;

        IsLoadingOrders = true;
        try
        {
            var (orders, total, _) = await _orderService.GetOrdersAsync(
                page: 1,
                limit: 3,
                sortField: "CREATED_TIME",
                sortOrder: "DESC"
            );

            // Build list on background thread
            var items = new ObservableCollection<DashboardOrderItem>();
            foreach (var order in orders)
            {
                items.Add(new DashboardOrderItem(
                    order.Id,
                    order.CustomerName,
                    order.Status,
                    order.Amount
                ));
            }

            // Assign on UI thread so PropertyChanged is raised correctly
            App.RunOnUIThread(() =>
            {
                RecentOrders = items;
                IsLoadingOrders = false;
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Dashboard] Failed to load recent orders: {ex.Message}");
            App.RunOnUIThread(() => IsLoadingOrders = false);
        }
    }

    /// <summary>
    /// Fetches low stock products from the backend.
    /// Only shows products with stock &lt; 5.
    /// </summary>
    private async Task LoadTopLowStockProductsAsync()
    {
        if (_graphql is null) return;

        try
        {
            // Fetch more than 5 to filter client-side
            var data = await _graphql.QueryAsync(
                @"query TopLowStock($limit: Int) {
                    topLowStockProducts(limit: $limit) {
                        product_id name stock images
                    }
                }",
                new { limit = 10 }
            );

            var products = data.GetProperty("topLowStockProducts").EnumerateArray();

            var items = new ObservableCollection<LowStockItem>();
            foreach (var p in products)
            {
                var name = p.GetProperty("name").GetString() ?? "";
                var stock = p.GetProperty("stock").GetInt32();
                var imageUrl = GetFirstImage(p);

                // Only show products with stock < 5
                if (stock < 5)
                {
                    items.Add(new LowStockItem(name, stock, imageUrl));
                }

                // Cap at 5 items for display
                if (items.Count >= 5) break;
            }

            App.RunOnUIThread(() =>
            {
                LowStockItems = items;
                LowStockBadgeText = $"{items.Count} items";
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Dashboard] Failed to load low stock products: {ex.Message}");
        }
    }

    /// <summary>
    /// Fetches top 5 best selling products from the backend (uses TopSellingProduct type with total_sold).
    /// </summary>
    private async Task LoadTopSellingProductsAsync()
    {
        if (_graphql is null) return;

        try
        {
            var data = await _graphql.QueryAsync(
                @"query TopSelling($limit: Int) {
                    topSellingProducts(limit: $limit) {
                        product_id name price images total_sold
                    }
                }",
                new { limit = 5 }
            );

            var products = data.GetProperty("topSellingProducts").EnumerateArray();

            var items = new ObservableCollection<BestSellingItem>();
            foreach (var p in products)
            {
                var name = p.GetProperty("name").GetString() ?? "";
                var sold = p.GetProperty("total_sold").GetInt32();
                var price = p.GetProperty("price").GetInt32();
                var priceText = $"{price:N0} ₫";
                var imageUrl = GetFirstImage(p);

                items.Add(new BestSellingItem(name, sold, priceText, imageUrl));
            }

            App.RunOnUIThread(() => BestSellingItems = items);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Dashboard] Failed to load best selling products: {ex.Message}");
        }
    }

    /// <summary>
    /// Fetches total product count for the stat card.
    /// </summary>
    private async Task LoadTotalProductsAsync()
    {
        if (_graphql is null) return;

        try
        {
            var data = await _graphql.QueryAsync(
                @"query { products(page: 1, limit: 1) { total } }"
            );

            var total = data.GetProperty("products").GetProperty("total").GetInt32();
            App.RunOnUIThread(() => TotalProducts = total.ToString("N0"));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Dashboard] Failed to load total products: {ex.Message}");
        }
    }

    /// <summary>
    /// Extracts the first image URL from a product's images array, or empty string.
    /// </summary>
    private static string GetFirstImage(JsonElement product)
    {
        if (product.TryGetProperty("images", out var images)
            && images.ValueKind == JsonValueKind.Array)
        {
            foreach (var img in images.EnumerateArray())
            {
                var url = img.GetString();
                if (!string.IsNullOrEmpty(url)) return url;
            }
        }
        return "";
    }

    /// <summary>
    /// Broadcasts a sidebar navigation request via WeakReferenceMessenger.
    /// Call ShellViewModel.RegisterNavigationHandler() to wire up the listener.
    /// </summary>
    public void NavigateTo(string key) =>
        WeakReferenceMessenger.Default.Send(new NavigateToPageMessage(key));
}

/// <summary>Message broadcasted when Dashboard requests navigation to another page.</summary>
public class NavigateToPageMessage(string pageKey)
{
    public string PageKey { get; } = pageKey;
}

/// <summary>Message to tell the Shell to show or hide its chrome (sidebar + top bar).</summary>
public class ChromeVisibilityMessage(bool showChrome)
{
    public bool ShowChrome { get; } = showChrome;
}

/// <summary>
/// Dashboard-specific order display model.
/// Uses same property names as XAML bindings: OrderId, Customer, Status, Total.
/// </summary>
public record DashboardOrderItem(string OrderId, string Customer, string Status, string Total)
{
    public SolidColorBrush StatusBgBrush =>
        Status switch
        {
            "Created" => new SolidColorBrush(Color.FromArgb(0xFF, 0xF9, 0xFA, 0xFB)),
            "Processing" => new SolidColorBrush(Color.FromArgb(0xFF, 0xEF, 0xF6, 0xFF)),
            "Shipped" => new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xF7, 0xE6)),
            "Delivered" => new SolidColorBrush(Color.FromArgb(0xFF, 0xEC, 0xFD, 0xF5)),
            "Cancelled" => new SolidColorBrush(Color.FromArgb(0xFF, 0xFE, 0xF2, 0xF2)),
            _ => new SolidColorBrush(Color.FromArgb(0xFF, 0xF9, 0xFA, 0xFB)),
        };

    public SolidColorBrush StatusBorderBrush =>
        Status switch
        {
            "Created" => new SolidColorBrush(Color.FromArgb(0xFF, 0xE5, 0xE7, 0xEB)),
            "Processing" => new SolidColorBrush(Color.FromArgb(0xFF, 0xBF, 0xDB, 0xFE)),
            "Shipped" => new SolidColorBrush(Color.FromArgb(0xFF, 0xFE, 0xD7, 0xAA)),
            "Delivered" => new SolidColorBrush(Color.FromArgb(0xFF, 0xA7, 0xF3, 0xD0)),
            "Cancelled" => new SolidColorBrush(Color.FromArgb(0xFF, 0xFE, 0xCA, 0xCA)),
            _ => new SolidColorBrush(Color.FromArgb(0xFF, 0xE5, 0xE7, 0xEB)),
        };

    public SolidColorBrush StatusFgBrush =>
        Status switch
        {
            "Created" => new SolidColorBrush(Color.FromArgb(0xFF, 0x37, 0x41, 0x51)),
            "Processing" => new SolidColorBrush(Color.FromArgb(0xFF, 0x25, 0x63, 0xEB)),
            "Shipped" => new SolidColorBrush(Color.FromArgb(0xFF, 0xD9, 0x77, 0x06)),
            "Delivered" => new SolidColorBrush(Color.FromArgb(0xFF, 0x05, 0x96, 0x69)),
            "Cancelled" => new SolidColorBrush(Color.FromArgb(0xFF, 0xDC, 0x26, 0x26)),
            _ => new SolidColorBrush(Color.FromArgb(0xFF, 0x37, 0x41, 0x51)),
        };
}

public record LowStockItem(string Name, int StockLeft, string ImagePath = "")
{
    public string StockMessage => $"Only {StockLeft} left";
    public BitmapImage? Thumbnail =>
        string.IsNullOrEmpty(ImagePath) ? null : new BitmapImage(new Uri(ImagePath));
}

public record BestSellingItem(string Name, int Sold, string Price, string ImagePath = "")
{
    public string SoldText => $"{Sold} sold";
    public BitmapImage? Thumbnail =>
        string.IsNullOrEmpty(ImagePath) ? null : new BitmapImage(new Uri(ImagePath));
}

/// <summary>
/// A single data point on the revenue chart.
/// </summary>
public record RevenueChartPoint(string Label, long Revenue, int Orders, string RawDate);

