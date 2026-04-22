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

    /// <summary>
    /// Parameterless constructor for design-time / fallback.
    /// </summary>
    public DashboardViewModel()
    {
        // Load fallback hardcoded data
        RecentOrders = new ObservableCollection<DashboardOrderItem>(new[]
        {
            new DashboardOrderItem("#ORD-7829", "Eleanor Pena", "Shipped", "$1,248.00"),
            new DashboardOrderItem("#ORD-7828", "Wade Warren", "Processing", "$854.00"),
            new DashboardOrderItem("#ORD-7827", "Esther Howard", "Delivered", "$2,450.00"),
        });
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
    private async Task LoadAllDashboardDataAsync()
    {
        await Task.WhenAll(
            LoadRecentOrdersAsync(),
            LoadTopLowStockProductsAsync(),
            LoadTopSellingProductsAsync(),
            LoadTotalProductsAsync()
        );
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

            // Update total orders stat card with real count
            TotalOrders = total.ToString("N0");

            // Map to dashboard-specific model
            RecentOrders.Clear();
            foreach (var order in orders)
            {
                RecentOrders.Add(new DashboardOrderItem(
                    order.Id,
                    order.CustomerName,
                    order.Status,
                    order.Amount
                ));
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Dashboard] Failed to load recent orders: {ex.Message}");
        }
        finally
        {
            IsLoadingOrders = false;
        }
    }

    /// <summary>
    /// Fetches top 5 low stock products from the backend.
    /// </summary>
    private async Task LoadTopLowStockProductsAsync()
    {
        if (_graphql is null) return;

        try
        {
            var data = await _graphql.QueryAsync(
                @"query TopLowStock($limit: Int) {
                    topLowStockProducts(limit: $limit) {
                        product_id name stock images
                    }
                }",
                new { limit = 5 }
            );

            var products = data.GetProperty("topLowStockProducts").EnumerateArray();

            LowStockItems.Clear();
            foreach (var p in products)
            {
                var name = p.GetProperty("name").GetString() ?? "";
                var stock = p.GetProperty("stock").GetInt32();
                var imageUrl = GetFirstImage(p);

                LowStockItems.Add(new LowStockItem(name, stock, imageUrl));
            }

            LowStockBadgeText = $"{LowStockItems.Count} items";
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

            BestSellingItems.Clear();
            foreach (var p in products)
            {
                var name = p.GetProperty("name").GetString() ?? "";
                var sold = p.GetProperty("total_sold").GetInt32();
                var price = p.GetProperty("price").GetInt32();
                var priceText = $"{price:N0} ₫";
                var imageUrl = GetFirstImage(p);

                BestSellingItems.Add(new BestSellingItem(name, sold, priceText, imageUrl));
            }
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
            TotalProducts = total.ToString("N0");
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

