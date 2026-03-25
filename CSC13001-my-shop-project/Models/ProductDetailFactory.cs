using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace CSC13001_my_shop_project.Models;

public static class ProductDetailFactory
{
    private static readonly SolidColorBrush RowAlt = new(Color.FromArgb(0xFF, 0xFA, 0xFA, 0xF8));
    private static readonly SolidColorBrush RowMain = new(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));

    public static (ProductModel Product, IReadOnlyList<OrderModel> Orders) Build(int productId)
    {
        var item = ProductCatalogData.FindById(productId);
        if (item is null)
            return BuildMissing(productId);

        if (item.Id == 9)
            return BuildCeramicShowcase(item);

        return BuildGeneric(item);
    }

    private static (ProductModel, IReadOnlyList<OrderModel>) BuildMissing(int id) =>
        (
            new ProductModel
            {
                Name = "Product not found",
                Category = "—",
                Sku = $"#{id}",
                Barcode = "—",
                ImagePath = "ms-appx:///Assets/Products/woven-basket.png",
                Rating = 0,
                ReviewCount = 0,
                Price = 0,
                CostPerItem = 0,
                MarginPercent = 0,
                Description = "This product is not in the sample catalog.",
                Tags = [],
                Weight = "—",
                Dimensions = "—",
                DateAdded = "—",
                TotalSold = 0,
                RevenueText = "$0",
                StockLeft = 0,
                StockTotal = 0,
                IsActive = false,
                StatusLabel = "Unknown",
                StatusBadgeBackground = new SolidColorBrush(Color.FromArgb(0xFF, 0xF3, 0xF4, 0xF6)),
                StatusBadgeForeground = new SolidColorBrush(Color.FromArgb(0xFF, 0x6B, 0x72, 0x80)),
                StatusDotBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0x9C, 0xA3, 0xAF)),
            },
            []
        );

    private static (ProductModel, IReadOnlyList<OrderModel>) BuildCeramicShowcase(ProductListItem item)
    {
        var product = new ProductModel
        {
            Name = item.Name,
            Category = item.Category.ToUpperInvariant(),
            Sku = "CPS-007",
            Barcode = "8901234567896",
            ImagePath = item.ImagePath,
            Rating = 4.6,
            ReviewCount = 19,
            Price = 55m,
            CostPerItem = 20m,
            MarginPercent = 64,
            Description =
                "A set of 3 handcrafted ceramic planters in graduated sizes. Features a speckled matte glaze in warm neutral tones. Drainage holes included. Perfect for succulents, herbs, or trailing plants.",
            Tags = ["planter", "ceramic", "set", "indoor", "plants"],
            Weight = "2.1 kg",
            Dimensions = "10/15/20 cm diameter",
            DateAdded = "January 8, 2026",
            TotalSold = 44,
            RevenueText = "$2,420",
            StockLeft = 30,
            StockTotal = 74,
            IsActive = true,
            StatusLabel = "Active",
            StatusBadgeBackground = new SolidColorBrush(Color.FromArgb(0xE6, 0xFF, 0xFF, 0xFF)),
            StatusBadgeForeground = new SolidColorBrush(Color.FromArgb(0xFF, 0x16, 0xA3, 0x4A)),
            StatusDotBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0x22, 0xC5, 0x5E)),
        };

        var orders = new List<OrderModel>
        {
            new("#ORD-7811", "Isabella Moore", "Feb 28, 2026", 1, "$249", "Delivered", RowMain),
            new("#ORD-7798", "Liam Harrison", "Feb 25, 2026", 2, "$498", "Shipped", RowAlt),
            new("#ORD-7784", "Sophia Chen", "Feb 22, 2026", 1, "$249", "Processing", RowMain),
            new("#ORD-7770", "Noah Williams", "Feb 19, 2026", 1, "$249", "Delivered", RowAlt),
            new("#ORD-7755", "Olivia Patel", "Feb 16, 2026", 3, "$747", "Delivered", RowMain),
        };

        return (product, orders);
    }

    private static (ProductModel, IReadOnlyList<OrderModel>) BuildGeneric(ProductListItem item)
    {
        var total = Math.Max(item.StockLeft + 20, item.StockLeft + 1);
        var sold = Math.Max(1, 100 - item.StockLeft);
        var revenue = item.Price * sold;

        var product = new ProductModel
        {
            Name = item.Name,
            Category = item.Category.ToUpperInvariant(),
            Sku = item.Sku,
            Barcode = $"89{item.Id:D10}",
            ImagePath = item.ImagePath,
            Rating = item.Rating,
            ReviewCount = item.ReviewCount,
            Price = item.Price,
            CostPerItem = Math.Round(item.Price * 0.45m, 0),
            MarginPercent = 55,
            Description =
                $"{item.Name} — sample catalog entry. High-quality materials and careful craftsmanship. See in-store displays for finish options.",
            Tags = [item.Category.ToLowerInvariant(), "featured", "home"],
            Weight = "1.0 kg",
            Dimensions = "Varies",
            DateAdded = "January 1, 2026",
            TotalSold = sold,
            RevenueText = $"${revenue:0}",
            StockLeft = item.StockLeft,
            StockTotal = total,
            IsActive = item.Status == ProductShelfStatus.Active,
            StatusLabel = item.StatusLabel,
            StatusBadgeBackground = item.StatusBadgeBg,
            StatusBadgeForeground = item.StatusBadgeFg,
            StatusDotBrush = item.StatusDotBrush,
        };

        var orders = new List<OrderModel>
        {
            new($"#ORD-{7800 + item.Id}", "Sample Customer A", "Feb 20, 2026", 1, $"${item.Price:0}", "Delivered", RowMain),
            new($"#ORD-{7790 + item.Id}", "Sample Customer B", "Feb 18, 2026", 2, $"${item.Price * 2:0}", "Shipped", RowAlt),
        };

        return (product, orders);
    }
}
