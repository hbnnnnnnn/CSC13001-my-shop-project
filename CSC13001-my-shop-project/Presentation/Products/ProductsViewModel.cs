using System.Collections.ObjectModel;
using System.Globalization;

namespace CSC13001_my_shop_project.Presentation.Products;

public sealed record PageButtonModel(int Page, bool IsCurrent);

public partial class ProductsViewModel : ObservableObject
{
    private const int DefaultPageSize = 8;

    private readonly IReadOnlyList<ProductListItem> _catalog;
    private List<ProductListItem> _filtered = [];

    public ProductsViewModel()
    {
        _catalog = BuildSampleCatalog();
        CategoryOptions = new ObservableCollection<string>(
            new[] { "All categories" }.Concat(
                _catalog.Select(p => p.Category).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(c => c)
            )
        );
        StatusFilterOptions = new ObservableCollection<string>(
            ["All status", "Active", "Low Stock", "Out of Stock"]
        );
        SortOptions = new ObservableCollection<string>(
            [
                "Newest First",
                "Name: A to Z",
                "Name: Z to A",
                "Price: Low to High",
                "Price: High to Low",
            ]
        );

        SelectedCategory = CategoryOptions[0];
        SelectedStatusFilter = StatusFilterOptions[0];
        SelectedSort = SortOptions[0];

        UpdateStats();
        ApplyFilters();
    }

    public ObservableCollection<string> CategoryOptions { get; }
    public ObservableCollection<string> StatusFilterOptions { get; }
    public ObservableCollection<string> SortOptions { get; }

    [ObservableProperty]
    private string searchQuery = string.Empty;

    [ObservableProperty]
    private string selectedCategory;

    [ObservableProperty]
    private string selectedStatusFilter;

    [ObservableProperty]
    private string selectedSort;

    [ObservableProperty]
    private int currentPage = 1;

    [ObservableProperty]
    private int totalPages = 1;

    [ObservableProperty]
    private bool isGridView = true;

    [ObservableProperty]
    private string totalProductsStat = "0";

    [ObservableProperty]
    private string activeStat = "0";

    [ObservableProperty]
    private string outOfStockStat = "0";

    [ObservableProperty]
    private string lowStockStat = "0";

    public ObservableCollection<ProductListItem> PagedItems { get; } = new();

    public ObservableCollection<PageButtonModel> PageButtons { get; } = new();

    public string PageSummaryText => $"Page {CurrentPage} of {TotalPages}";

    public bool CanGoPrevious => CurrentPage > 1;

    public bool CanGoNext => CurrentPage < TotalPages;

    partial void OnSearchQueryChanged(string value) => ApplyFilters();

    partial void OnSelectedCategoryChanged(string value) => ApplyFilters();

    partial void OnSelectedStatusFilterChanged(string value) => ApplyFilters();

    partial void OnSelectedSortChanged(string value) => ApplyFilters();

    [RelayCommand]
    private void ToggleGridView()
    {
        IsGridView = true;
    }

    [RelayCommand]
    private void ToggleListView()
    {
        IsGridView = false;
    }

    [RelayCommand]
    private void PreviousPage()
    {
        if (CurrentPage <= 1)
            return;
        CurrentPage--;
        RebuildCurrentPage();
    }

    [RelayCommand]
    private void NextPage()
    {
        if (CurrentPage >= TotalPages)
            return;
        CurrentPage++;
        RebuildCurrentPage();
    }

    [RelayCommand]
    private void GoToPage(object? pageParam)
    {
        var page = pageParam switch
        {
            int i => i,
            long l => (int)l,
            string s when int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) => n,
            _ => 0,
        };
        if (page < 1 || page > TotalPages)
            return;
        CurrentPage = page;
        RebuildCurrentPage();
    }

    [RelayCommand]
    private void AddNewProduct()
    {
        // Placeholder for future create-product flow
    }

    private void UpdateStats()
    {
        TotalProductsStat = _catalog.Count.ToString(CultureInfo.InvariantCulture);
        ActiveStat = _catalog.Count(p => p.Status == ProductShelfStatus.Active).ToString(CultureInfo.InvariantCulture);
        OutOfStockStat = _catalog.Count(p => p.Status == ProductShelfStatus.OutOfStock).ToString(CultureInfo.InvariantCulture);
        LowStockStat = _catalog.Count(p => p.Status == ProductShelfStatus.LowStock).ToString(CultureInfo.InvariantCulture);
    }

    private void ApplyFilters()
    {
        IEnumerable<ProductListItem> q = _catalog;

        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            var s = SearchQuery.Trim();
            q = q.Where(p =>
                p.Name.Contains(s, StringComparison.OrdinalIgnoreCase)
                || p.Sku.Contains(s, StringComparison.OrdinalIgnoreCase)
                || p.Category.Contains(s, StringComparison.OrdinalIgnoreCase)
            );
        }

        if (!string.IsNullOrEmpty(SelectedCategory) && SelectedCategory != "All categories")
            q = q.Where(p => string.Equals(p.Category, SelectedCategory, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrEmpty(SelectedStatusFilter) && SelectedStatusFilter != "All status")
        {
            q = SelectedStatusFilter switch
            {
                "Active" => q.Where(p => p.Status == ProductShelfStatus.Active),
                "Low Stock" => q.Where(p => p.Status == ProductShelfStatus.LowStock),
                "Out of Stock" => q.Where(p => p.Status == ProductShelfStatus.OutOfStock),
                _ => q,
            };
        }

        q = SelectedSort switch
        {
            "Name: A to Z" => q.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase),
            "Name: Z to A" => q.OrderByDescending(p => p.Name, StringComparer.OrdinalIgnoreCase),
            "Price: Low to High" => q.OrderBy(p => p.Price).ThenBy(p => p.Name),
            "Price: High to Low" => q.OrderByDescending(p => p.Price).ThenBy(p => p.Name),
            _ => q.OrderByDescending(p => p.Id),
        };

        _filtered = q.ToList();
        TotalPages = Math.Max(1, (int)Math.Ceiling(_filtered.Count / (double)DefaultPageSize));
        if (CurrentPage > TotalPages)
            CurrentPage = TotalPages;
        if (CurrentPage < 1)
            CurrentPage = 1;

        RebuildCurrentPage();
    }

    private void RebuildPageButtons()
    {
        PageButtons.Clear();
        for (var i = 1; i <= TotalPages; i++)
            PageButtons.Add(new PageButtonModel(i, i == CurrentPage));
    }

    private void RebuildCurrentPage()
    {
        PagedItems.Clear();
        var skip = (CurrentPage - 1) * DefaultPageSize;
        foreach (var item in _filtered.Skip(skip).Take(DefaultPageSize))
            PagedItems.Add(item);

        RebuildPageButtons();

        OnPropertyChanged(nameof(PageSummaryText));
        OnPropertyChanged(nameof(CanGoPrevious));
        OnPropertyChanged(nameof(CanGoNext));
    }

    private static IReadOnlyList<ProductListItem> BuildSampleCatalog()
    {
        static string PickImage(int index) =>
            (index % 6) switch
            {
                0 => "ms-appx:///Assets/Products/woven-basket.png",
                1 => "ms-appx:///Assets/Products/teak-coffee-table.png",
                2 => "ms-appx:///Assets/Products/bamboo-floor-lamp.png",
                3 => "ms-appx:///Assets/Products/ceramic-planter.png",
                4 => "ms-appx:///Assets/Products/beige-modular-sofa.png",
                _ => "ms-appx:///Assets/Products/woven-basket.png",
            };

        return
        [
            new(12, "Woven Storage Basket", "Storage", "WSB-010", PickImage(0), 4.5, 48, 38m, 150m, 60, ProductShelfStatus.Active),
            new(11, "Teak Coffee Table", "Furniture", "TCT-204", PickImage(1), 4.8, 112, 249m, null, 8, ProductShelfStatus.LowStock),
            new(10, "Bamboo Floor Lamp", "Lighting", "BFL-088", PickImage(2), 4.2, 36, 89m, 120m, 0, ProductShelfStatus.OutOfStock),
            new(9, "Ceramic Planter Set", "Decor", "CPS-331", PickImage(3), 4.6, 67, 45m, null, 24, ProductShelfStatus.Active),
            new(8, "Beige Modular Sofa", "Furniture", "BMS-901", PickImage(4), 4.9, 203, 899m, 1099m, 3, ProductShelfStatus.LowStock),
            new(7, "Linen Woven Rug", "Textiles", "LWR-445", PickImage(5), 4.1, 19, 120m, null, 6, ProductShelfStatus.LowStock),
            new(6, "Nordic Lounge Chair", "Furniture", "NLC-112", PickImage(1), 4.7, 89, 249m, 299m, 0, ProductShelfStatus.OutOfStock),
            new(5, "Minimalist Wooden Lamp", "Lighting", "MWL-077", PickImage(2), 4.0, 54, 79m, null, 42, ProductShelfStatus.Active),
            new(4, "Oak Side Table", "Furniture", "OST-556", PickImage(1), 4.3, 31, 129m, 159m, 4, ProductShelfStatus.LowStock),
            new(3, "Glass Vase Trio", "Decor", "GVT-223", PickImage(3), 4.4, 22, 55m, null, 11, ProductShelfStatus.Active),
            new(2, "Cotton Throw Blanket", "Textiles", "CTB-668", PickImage(5), 4.8, 140, 68m, 85m, 2, ProductShelfStatus.LowStock),
            new(1, "Ceramic Artisan Vase", "Decor", "CAV-019", PickImage(3), 4.5, 98, 45m, null, 30, ProductShelfStatus.Active),
        ];
    }
}
