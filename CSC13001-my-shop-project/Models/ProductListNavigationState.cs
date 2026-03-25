namespace CSC13001_my_shop_project.Models;

public sealed class ProductListNavigationState
{
    public double ScrollOffsetY { get; set; }
    public string? SearchQuery { get; set; }
    public string? SelectedCategory { get; set; }
    public string? SelectedStatusFilter { get; set; }
    public string? SelectedSort { get; set; }
    public int CurrentPage { get; set; } = 1;
    public bool IsGridView { get; set; } = true;
    public int? LastViewedProductId { get; set; }
}
