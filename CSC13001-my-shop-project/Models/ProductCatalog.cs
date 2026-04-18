using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.UI;

namespace CSC13001_my_shop_project.Models;

public enum ProductShelfStatus
{
    Active,
    LowStock,
    OutOfStock,
}

/// <summary>Row model for the All Products grid/list (sample / future API mapping).</summary>
public sealed class ProductListItem
{
    public ProductListItem(
        int id,
        string name,
        string category,
        string sku,
        string imagePath,
        double rating,
        int reviewCount,
        decimal price,
        decimal? compareAtPrice,
        int stockLeft,
        ProductShelfStatus status,
        DateTime? apiCreatedAt = null,
        string? graphQlProductId = null
    )
    {
        Id = id;
        Name = name;
        Category = category;
        Sku = sku;
        ImagePath = imagePath;
        Rating = rating;
        ReviewCount = reviewCount;
        Price = price;
        CompareAtPrice = compareAtPrice;
        StockLeft = stockLeft;
        Status = status;
        ApiCreatedAt = apiCreatedAt;
        GraphQlProductId = graphQlProductId;
    }

    public int Id { get; }

    /// <summary>Server <c>created_time</c> when loaded from GraphQL; used for client-side sort.</summary>
    public DateTime? ApiCreatedAt { get; }

    /// <summary>Backend <c>product_id</c>; use for navigation/API instead of <see cref="Id"/> when set.</summary>
    public string? GraphQlProductId { get; }
    public string Name { get; }
    public string Category { get; }
    public string Sku { get; }
    public string ImagePath { get; }
    public double Rating { get; }
    public int ReviewCount { get; }
    public decimal Price { get; }
    public decimal? CompareAtPrice { get; }
    public int StockLeft { get; }
    public ProductShelfStatus Status { get; }

    public string CategorySkuLine => $"{Category.ToUpperInvariant()} — {Sku}";

    public string RatingText => $"{Rating:0.0} ({ReviewCount})";

    public string PriceText => $"${Price:0}";

    public string? CompareAtText => CompareAtPrice is { } c && c > Price ? $"${c:0}" : null;

    public bool ShowCompareAt => CompareAtText is not null;

    public string StockText => $"{StockLeft} left";

    public string StatusLabel =>
        Status switch
        {
            ProductShelfStatus.Active => "Active",
            ProductShelfStatus.LowStock => "Low Stock",
            ProductShelfStatus.OutOfStock => "Out of Stock",
            _ => "",
        };

    public SolidColorBrush StatusDotBrush =>
        Status switch
        {
            ProductShelfStatus.Active => new SolidColorBrush(
                Color.FromArgb(0xFF, 0x22, 0xC5, 0x5E)
            ),
            ProductShelfStatus.LowStock => new SolidColorBrush(
                Color.FromArgb(0xFF, 0xF5, 0x9E, 0x0B)
            ),
            ProductShelfStatus.OutOfStock => new SolidColorBrush(
                Color.FromArgb(0xFF, 0xEF, 0x44, 0x44)
            ),
            _ => new SolidColorBrush(Color.FromArgb(0xFF, 0x9C, 0xA3, 0xAF)),
        };

    public SolidColorBrush StatusBadgeBg =>
        Status switch
        {
            ProductShelfStatus.Active => new SolidColorBrush(
                Color.FromArgb(0xE6, 0xFF, 0xFF, 0xFF)
            ),
            ProductShelfStatus.LowStock => new SolidColorBrush(
                Color.FromArgb(0xE6, 0xFF, 0xFB, 0xEB)
            ),
            ProductShelfStatus.OutOfStock => new SolidColorBrush(
                Color.FromArgb(0xE6, 0xFE, 0xF2, 0xF2)
            ),
            _ => new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF)),
        };

    public SolidColorBrush StatusBadgeFg =>
        Status switch
        {
            ProductShelfStatus.Active => new SolidColorBrush(
                Color.FromArgb(0xFF, 0x16, 0xA3, 0x4A)
            ),
            ProductShelfStatus.LowStock => new SolidColorBrush(
                Color.FromArgb(0xFF, 0xD9, 0x77, 0x06)
            ),
            ProductShelfStatus.OutOfStock => new SolidColorBrush(
                Color.FromArgb(0xFF, 0xDC, 0x26, 0x26)
            ),
            _ => new SolidColorBrush(Color.FromArgb(0xFF, 0x9C, 0xA3, 0xAF)),
        };

    public BitmapImage? Thumbnail =>
        string.IsNullOrEmpty(ImagePath) ? null : new BitmapImage(new Uri(ImagePath));
}
