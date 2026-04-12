using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.UI;
using CSC13001_my_shop_project.Services;

namespace CSC13001_my_shop_project.Presentation.Dashboard;

public partial class DashboardViewModel : ObservableObject
{
    private readonly OrderService? _orderService;

    [ObservableProperty]
    private string totalProducts = "1,248";

    [ObservableProperty]
    private string totalOrders = "567";

    [ObservableProperty]
    private string totalRevenue = "$124,560";

    [ObservableProperty]
    private ObservableCollection<DashboardOrderItem> _recentOrders = new();

    [ObservableProperty]
    private bool _isLoadingOrders;

    public IReadOnlyList<LowStockItem> LowStockItems { get; } =
        new List<LowStockItem>
        {
            new("Teak Coffee Table", 3, "ms-appx:///Assets/Products/teak-coffee-table.png"),
            new("Bamboo Floor Lamp", 5, "ms-appx:///Assets/Products/bamboo-floor-lamp.png"),
            new("Ceramic Planter", 2, "ms-appx:///Assets/Products/ceramic-planter.png"),
            new("Woven Basket", 4, "ms-appx:///Assets/Products/woven-basket.png"),
            new("Oak Side Table", 1, ""),
        };

    public IReadOnlyList<BestSellingItem> BestSellingItems { get; } =
        new List<BestSellingItem>
        {
            new("Nordic Lounge Chair", 324, "$249", ""),
            new("Minimalist Wooden Lamp", 212, "$89", ""),
            new(
                "Beige Modular Sofa",
                156,
                "$899",
                "ms-appx:///Assets/Products/beige-modular-sofa.png"
            ),
            new(
                "Ceramic Artisan Vase",
                98,
                "$45",
                "ms-appx:///Assets/Products/ceramic-planter.png"
            ),
            new("Linen Woven Rug", 74, "$120", "ms-appx:///Assets/Products/woven-basket.png"),
        };

    public string LowStockBadgeText => $"{LowStockItems.Count} items";

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
    /// Constructor with DI — receives OrderService to fetch real data.
    /// </summary>
    public DashboardViewModel(OrderService orderService) : this()
    {
        _orderService = orderService;
        _ = LoadRecentOrdersAsync();
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
            // Keep fallback data on error
        }
        finally
        {
            IsLoadingOrders = false;
        }
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
