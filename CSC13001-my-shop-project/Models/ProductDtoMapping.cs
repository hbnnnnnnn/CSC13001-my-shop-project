using System.Globalization;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace CSC13001_my_shop_project.Models;

public static class ProductDtoMapping
{
    public static ProductListItem ToListItem(ProductDto p)
    {
        var id = int.TryParse(p.ProductId, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n)
            ? n
            : 0;

        DateTime? created = null;
        if (!string.IsNullOrWhiteSpace(p.CreatedTime)
            && DateTime.TryParse(
                p.CreatedTime,
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind,
                out var parsed))
        {
            created = parsed;
        }

        var status = p.Stock <= 0
            ? ProductShelfStatus.OutOfStock
            : p.Stock < 10
                ? ProductShelfStatus.LowStock
                : ProductShelfStatus.Active;

        var image = p.Images is { Count: > 0 } ? p.Images[0] : "";
        var category = p.Category?.Name ?? "";

        return new ProductListItem(
            id,
            p.Name,
            category,
            p.Sku,
            image,
            0,
            0,
            p.Price,
            null,
            p.Stock,
            status,
            created,
            string.IsNullOrEmpty(p.ProductId) ? null : p.ProductId
        );
    }

    public static ProductModel ToDetailModel(ProductDto p)
    {
        var row = ToListItem(p);
        var tags = new List<string>();
        if (!string.IsNullOrWhiteSpace(p.Category?.Name))
            tags.Add(p.Category.Name.ToLowerInvariant());
        if (!string.IsNullOrWhiteSpace(p.Supplier))
            tags.Add(p.Supplier.Trim());

        return new ProductModel
        {
            Name = p.Name,
            Category = (p.Category?.Name ?? row.Category).ToUpperInvariant(),
            Sku = p.Sku,
            Barcode = p.ProductId,
            ImagePath = row.ImagePath,
            Rating = 0,
            ReviewCount = 0,
            Price = p.Price,
            CostPerItem = 0,
            MarginPercent = 0,
            Description = string.IsNullOrWhiteSpace(p.Description) ? "—" : p.Description.Trim(),
            Tags = tags,
            Weight = "—",
            Dimensions = "—",
            DateAdded = string.IsNullOrWhiteSpace(p.CreatedTime) ? "—" : p.CreatedTime,
            TotalSold = 0,
            RevenueText = "$0",
            StockLeft = p.Stock,
            StockTotal = Math.Max(p.Stock, 1),
            IsActive = row.Status == ProductShelfStatus.Active,
            StatusLabel = row.StatusLabel,
            StatusBadgeBackground = row.StatusBadgeBg,
            StatusBadgeForeground = row.StatusBadgeFg,
            StatusDotBrush = row.StatusDotBrush,
        };
    }

    public static ProductModel DetailLoadingModel() =>
        new()
        {
            Name = "Loading…",
            Category = "",
            Sku = "",
            Barcode = "",
            ImagePath = "",
            Rating = 0,
            ReviewCount = 0,
            Price = 0,
            CostPerItem = 0,
            MarginPercent = 0,
            Description = "",
            Tags = [],
            Weight = "—",
            Dimensions = "—",
            DateAdded = "—",
            TotalSold = 0,
            RevenueText = "$0",
            StockLeft = 0,
            StockTotal = 1,
            IsActive = true,
            StatusLabel = "…",
            StatusBadgeBackground = new SolidColorBrush(Color.FromArgb(0xE6, 0xFF, 0xFF, 0xFF)),
            StatusBadgeForeground = new SolidColorBrush(Color.FromArgb(0xFF, 0x6B, 0x72, 0x80)),
            StatusDotBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0x9C, 0xA3, 0xAF)),
        };

    public static ProductModel DetailNotFoundModel(string id) =>
        new()
        {
            Name = "Product not found",
            Category = "—",
            Sku = "—",
            Barcode = id,
            ImagePath = "",
            Rating = 0,
            ReviewCount = 0,
            Price = 0,
            CostPerItem = 0,
            MarginPercent = 0,
            Description = $"No product with id \"{id}\".",
            Tags = [],
            Weight = "—",
            Dimensions = "—",
            DateAdded = "—",
            TotalSold = 0,
            RevenueText = "$0",
            StockLeft = 0,
            StockTotal = 1,
            IsActive = false,
            StatusLabel = "Unknown",
            StatusBadgeBackground = new SolidColorBrush(Color.FromArgb(0xFF, 0xF3, 0xF4, 0xF6)),
            StatusBadgeForeground = new SolidColorBrush(Color.FromArgb(0xFF, 0x6B, 0x72, 0x80)),
            StatusDotBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0x9C, 0xA3, 0xAF)),
        };

    public static ProductModel DetailErrorModel(string message) =>
        new()
        {
            Name = "Could not load product",
            Category = "—",
            Sku = "—",
            Barcode = "—",
            ImagePath = "",
            Rating = 0,
            ReviewCount = 0,
            Price = 0,
            CostPerItem = 0,
            MarginPercent = 0,
            Description = message,
            Tags = [],
            Weight = "—",
            Dimensions = "—",
            DateAdded = "—",
            TotalSold = 0,
            RevenueText = "$0",
            StockLeft = 0,
            StockTotal = 1,
            IsActive = false,
            StatusLabel = "Error",
            StatusBadgeBackground = new SolidColorBrush(Color.FromArgb(0xE6, 0xFE, 0xF2, 0xF2)),
            StatusBadgeForeground = new SolidColorBrush(Color.FromArgb(0xFF, 0xDC, 0x26, 0x26)),
            StatusDotBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0xEF, 0x44, 0x44)),
        };
}
