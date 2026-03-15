using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.UI;

namespace CSC13001_my_shop_project.Presentation.Dashboard;

public partial class DashboardViewModel : ObservableObject
{
    [ObservableProperty]
    private string totalProducts = "1,248";

    [ObservableProperty]
    private string totalOrders = "567";

    [ObservableProperty]
    private string totalRevenue = "$124,560";

    public IReadOnlyList<OrderItem> RecentOrders { get; } = new List<OrderItem>
    {
        new("#ORD-7829", "Eleanor Pena",  "Shipped",    "$1,248.00"),
        new("#ORD-7828", "Wade Warren",   "Processing", "$854.00"),
        new("#ORD-7827", "Esther Howard", "Delivered",  "$2,450.00"),
    };

    public IReadOnlyList<LowStockItem> LowStockItems { get; } = new List<LowStockItem>
    {
        new("Teak Coffee Table",  3, "ms-appx:///Assets/Products/teak-coffee-table.png"),
        new("Bamboo Floor Lamp",  5, "ms-appx:///Assets/Products/bamboo-floor-lamp.png"),
        new("Ceramic Planter",    2, "ms-appx:///Assets/Products/ceramic-planter.png"),
        new("Woven Basket",       4, "ms-appx:///Assets/Products/woven-basket.png"),
        new("Oak Side Table",     1, ""),
    };

    public IReadOnlyList<BestSellingItem> BestSellingItems { get; } = new List<BestSellingItem>
    {
        new("Nordic Lounge Chair",    324, "$249", ""),
        new("Minimalist Wooden Lamp", 212, "$89",  ""),
        new("Beige Modular Sofa",     156, "$899", "ms-appx:///Assets/Products/beige-modular-sofa.png"),
        new("Ceramic Artisan Vase",    98, "$45",  "ms-appx:///Assets/Products/ceramic-planter.png"),
        new("Linen Woven Rug",         74, "$120", "ms-appx:///Assets/Products/woven-basket.png"),
    };

    public string LowStockBadgeText => $"{LowStockItems.Count} items";

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

public record OrderItem(string OrderId, string Customer, string Status, string Total)
{
    public SolidColorBrush StatusBgBrush => Status switch
    {
        "Shipped"    => new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xF7, 0xE6)),
        "Processing" => new SolidColorBrush(Color.FromArgb(0xFF, 0xEF, 0xF6, 0xFF)),
        "Delivered"  => new SolidColorBrush(Color.FromArgb(0xFF, 0xEC, 0xFD, 0xF5)),
        _            => new SolidColorBrush(Color.FromArgb(0xFF, 0xF9, 0xFA, 0xFB)),
    };

    public SolidColorBrush StatusBorderBrush => Status switch
    {
        "Shipped"    => new SolidColorBrush(Color.FromArgb(0xFF, 0xFE, 0xD7, 0xAA)),
        "Processing" => new SolidColorBrush(Color.FromArgb(0xFF, 0xBF, 0xDB, 0xFE)),
        "Delivered"  => new SolidColorBrush(Color.FromArgb(0xFF, 0xA7, 0xF3, 0xD0)),
        _            => new SolidColorBrush(Color.FromArgb(0xFF, 0xE5, 0xE7, 0xEB)),
    };

    public SolidColorBrush StatusFgBrush => Status switch
    {
        "Shipped"    => new SolidColorBrush(Color.FromArgb(0xFF, 0xD9, 0x77, 0x06)),
        "Processing" => new SolidColorBrush(Color.FromArgb(0xFF, 0x25, 0x63, 0xEB)),
        "Delivered"  => new SolidColorBrush(Color.FromArgb(0xFF, 0x05, 0x96, 0x69)),
        _            => new SolidColorBrush(Color.FromArgb(0xFF, 0x37, 0x41, 0x51)),
    };
}

public record LowStockItem(string Name, int StockLeft, string ImagePath = "")
{
    public string StockMessage => $"Only {StockLeft} left";
    public BitmapImage? Thumbnail => string.IsNullOrEmpty(ImagePath)
        ? null
        : new BitmapImage(new Uri(ImagePath));
}

public record BestSellingItem(string Name, int Sold, string Price, string ImagePath = "")
{
    public string SoldText => $"{Sold} sold";
    public BitmapImage? Thumbnail => string.IsNullOrEmpty(ImagePath)
        ? null
        : new BitmapImage(new Uri(ImagePath));
}
