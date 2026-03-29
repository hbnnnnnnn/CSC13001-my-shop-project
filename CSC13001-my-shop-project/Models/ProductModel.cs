using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

namespace CSC13001_my_shop_project.Models;

public sealed class ProductModel
{
    public required string Name { get; init; }
    public required string Category { get; init; }
    public required string Sku { get; init; }
    public required string Barcode { get; init; }
    public required string ImagePath { get; init; }
    public double Rating { get; init; }
    public int ReviewCount { get; init; }
    public decimal Price { get; init; }
    public decimal CostPerItem { get; init; }
    public int MarginPercent { get; init; }
    public required string Description { get; init; }
    public required IReadOnlyList<string> Tags { get; init; }
    public required string Weight { get; init; }
    public required string Dimensions { get; init; }
    public required string DateAdded { get; init; }
    public int TotalSold { get; init; }
    public string RevenueText { get; init; } = "";
    public int StockLeft { get; init; }
    public int StockTotal { get; init; }
    public bool IsActive { get; init; }
    public required string StatusLabel { get; init; }
    public SolidColorBrush StatusBadgeBackground { get; init; } = null!;
    public SolidColorBrush StatusBadgeForeground { get; init; } = null!;
    public SolidColorBrush StatusDotBrush { get; init; } = null!;

    public string PriceText => $"${Price:0}";
    public string CostMarginLine => $"Cost per item: ${CostPerItem:0} · Margin: {MarginPercent}%";
    public string RatingReviewsText => $"{Rating:0.0} ({ReviewCount} reviews)";
    public string StockLevelText => $"{StockLeft} / {StockTotal}";

    public BitmapImage? Thumbnail =>
        string.IsNullOrEmpty(ImagePath) ? null : new BitmapImage(new Uri(ImagePath));
}
