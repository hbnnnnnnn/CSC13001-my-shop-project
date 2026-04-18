using System.Collections.ObjectModel;
using System.Globalization;
using CSC13001_my_shop_project.Models;
using CSC13001_my_shop_project.Services;

namespace CSC13001_my_shop_project.Presentation.Products;

public sealed record PageButtonModel(int Page, bool IsCurrent);

public partial class ProductsViewModel : ObservableObject
{
    private const int DefaultPageSize = 8;

    private readonly List<ProductListItem> _catalog;
    private List<ProductListItem> _filtered = [];

    public ProductsViewModel()
    {
        _catalog = [..ProductCatalogData.Items];
        CategoryOptions = new ObservableCollection<string>(
            new[] { "All categories" }.Concat(
                _catalog
                    .Select(p => p.Category)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(c => c)
            )
        );
        StatusFilterOptions = new ObservableCollection<string>([
            "All status",
            "Active",
            "Low Stock",
            "Out of Stock",
        ]);
        SortOptions = new ObservableCollection<string>([
            "Newest First",
            "Name: A to Z",
            "Name: Z to A",
            "Price: Low to High",
            "Price: High to Low",
        ]);

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

    private bool _suppressFilters;

    partial void OnSearchQueryChanged(string value)
    {
        if (!_suppressFilters)
            ApplyFilters();
    }

    partial void OnSelectedCategoryChanged(string value)
    {
        if (!_suppressFilters)
            ApplyFilters();
    }

    partial void OnSelectedStatusFilterChanged(string value)
    {
        if (!_suppressFilters)
            ApplyFilters();
    }

    partial void OnSelectedSortChanged(string value)
    {
        if (!_suppressFilters)
            ApplyFilters();
    }

    public void BulkRestoreState(
        string? search,
        string? category,
        string? statusFilter,
        string? sort,
        int page,
        bool gridView
    )
    {
        _suppressFilters = true;
        SearchQuery = search ?? string.Empty;
        SelectedCategory = category ?? CategoryOptions[0];
        SelectedStatusFilter = statusFilter ?? StatusFilterOptions[0];
        SelectedSort = sort ?? SortOptions[0];
        IsGridView = gridView;
        _suppressFilters = false;

        ApplyFilters();

        if (page >= 1 && page <= TotalPages)
        {
            CurrentPage = page;
            RebuildCurrentPage();
        }
    }

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
            string s
                when int.TryParse(
                    s,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out var n
                ) => n,
            _ => 0,
        };
        if (page < 1 || page > TotalPages)
            return;
        CurrentPage = page;
        RebuildCurrentPage();
    }

    /// <summary>Appends a user-created product and refreshes filters and category list.</summary>
    public void AddProduct(NewProductForm form)
    {
        var nextId = _catalog.Count == 0 ? 1 : _catalog.Max(p => p.Id) + 1;
        var image = ProductCatalogData.PickImageByIndex(nextId);
        _catalog.Add(
            new ProductListItem(
                nextId,
                form.Name,
                form.Category,
                form.Sku,
                image,
                4.5,
                0,
                form.Price,
                null,
                form.Stock,
                form.Status
            )
        );
        RebuildCategoryOptions();
        UpdateStats();
        ApplyFilters();
    }

    /// <summary>Merges a product returned from the API after <see cref="CreateProductViewModel"/> succeeds.</summary>
    public void MergeCreatedProduct(CreatedProductSummary p)
    {
        var status =
            p.Stock == 0
                ? ProductShelfStatus.OutOfStock
                : p.Stock < 10
                    ? ProductShelfStatus.LowStock
                    : ProductShelfStatus.Active;
        var thumb =
            string.IsNullOrEmpty(p.FirstImageUrl)
                ? ProductCatalogData.PickImageByIndex(p.Id)
                : p.FirstImageUrl;
        _catalog.Add(
            new ProductListItem(
                p.Id,
                p.Name,
                p.CategoryName,
                p.Sku,
                thumb,
                4.5,
                0,
                (decimal)p.Price,
                null,
                p.Stock,
                status
            )
        );
        RebuildCategoryOptions();
        UpdateStats();
        ApplyFilters();
    }

    private void RebuildCategoryOptions()
    {
        var keepCategory = SelectedCategory;
        CategoryOptions.Clear();
        CategoryOptions.Add("All categories");
        foreach (
            var c in _catalog
                .Select(p => p.Category)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(c => c, StringComparer.OrdinalIgnoreCase)
        )
            CategoryOptions.Add(c);

        SelectedCategory = CategoryOptions.Contains(keepCategory)
            ? keepCategory
            : CategoryOptions[0];
    }

    private void UpdateStats()
    {
        TotalProductsStat = _catalog.Count.ToString(CultureInfo.InvariantCulture);
        ActiveStat = _catalog
            .Count(p => p.Status == ProductShelfStatus.Active)
            .ToString(CultureInfo.InvariantCulture);
        OutOfStockStat = _catalog
            .Count(p => p.Status == ProductShelfStatus.OutOfStock)
            .ToString(CultureInfo.InvariantCulture);
        LowStockStat = _catalog
            .Count(p => p.Status == ProductShelfStatus.LowStock)
            .ToString(CultureInfo.InvariantCulture);
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
            q = q.Where(p =>
                string.Equals(p.Category, SelectedCategory, StringComparison.OrdinalIgnoreCase)
            );

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
}
