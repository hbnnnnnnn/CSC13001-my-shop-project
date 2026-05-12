using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CSC13001_my_shop_project.Models;
using CSC13001_my_shop_project.Services;

namespace CSC13001_my_shop_project.Presentation.Products;

public sealed record PageButtonModel(int Page, bool IsCurrent);

public partial class ProductsViewModel : ObservableObject
{
    private const int DefaultPageSize = 10;

    private readonly IProductService _productService;
    private readonly IImageUploadService _imageUpload;
    private List<ProductListItem> _catalog = [];
    private List<ProductListItem> _filtered = [];

    public ProductsViewModel(IProductService productService, IImageUploadService imageUpload)
    {
        _productService = productService;
        _imageUpload = imageUpload;
        _catalog = [];
        CategoryOptions = new ObservableCollection<string>(["All categories"]);
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
        _ = LoadCategoryOptionsFromApiAsync();
        _ = ReloadCatalogFromApiAsync();
    }

    public ObservableCollection<string> CategoryOptions { get; } = new();
    public ObservableCollection<string> StatusFilterOptions { get; }
    public ObservableCollection<string> SortOptions { get; }

    [ObservableProperty]
    private bool isCreateDialogOpen;

    [ObservableProperty]
    private CreateProductViewModel? createDialogViewModel;

    /// <summary>
    /// When false, <see cref="OpenCreateProductDialogCommand"/> cannot run — avoids pointer carry-over
    /// from sidebar navigation opening the modal on the first frame.
    /// </summary>
    [ObservableProperty]
    private bool isReadyForCreateDialog;

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
    private int pageSize = DefaultPageSize;

    public ObservableCollection<int> PageOptions { get; } = new() { 1 };

    public List<int> PageSizeOptions { get; } = new() { 5, 10, 20 };

    partial void OnPageSizeChanged(int value)
    {
        if (_suppressFilters)
            return;
        CurrentPage = 1;
        ApplyFilters();
    }

    partial void OnCurrentPageChanged(int value)
    {
        if (_suppressFilters)
            return;
        RebuildCurrentPage();
    }

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

    [ObservableProperty]
    private bool isLoadingCatalog = true;

    [ObservableProperty]
    private string catalogLoadError = string.Empty;

    public bool HasCatalogLoadError => !string.IsNullOrEmpty(CatalogLoadError);

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

    partial void OnIsReadyForCreateDialogChanged(bool value) =>
        OpenCreateProductDialogCommand.NotifyCanExecuteChanged();

    partial void OnIsCreateDialogOpenChanged(bool value) =>
        OpenCreateProductDialogCommand.NotifyCanExecuteChanged();

    partial void OnCatalogLoadErrorChanged(string value) =>
        OnPropertyChanged(nameof(HasCatalogLoadError));

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

    private static IEnumerable<string> BuildCategoryOptionNames(List<ProductListItem> items)
    {
        yield return "All categories";
        foreach (var name in items.Select(p => p.Category).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(c => c))
            yield return name;
    }

    private void RebuildCategoryOptions()
    {
        CategoryOptions.Clear();
        foreach (var c in BuildCategoryOptionNames(_catalog))
            CategoryOptions.Add(c);
        if (!CategoryOptions.Contains(SelectedCategory))
            SelectedCategory = CategoryOptions[0];
    }

    private void MergeCategoryOptionsFromCatalog()
    {
        var existing = new HashSet<string>(CategoryOptions, StringComparer.OrdinalIgnoreCase);
        foreach (var name in BuildCategoryOptionNames(_catalog))
        {
            if (!existing.Contains(name))
                CategoryOptions.Add(name);
        }

        if (!CategoryOptions.Contains(SelectedCategory))
            SelectedCategory = CategoryOptions[0];
    }

    private async Task LoadCategoryOptionsFromApiAsync()
    {
        try
        {
            var list = await _productService.GetCategoriesAsync().ConfigureAwait(false);
            App.RunOnUIThread(() => ApplyCategoryOptionsFromApi(list));
        }
        catch
        {
            // Fallback to categories derived from catalog.
        }
    }

    public Task RefreshCategoriesAsync() => LoadCategoryOptionsFromApiAsync();

    public Task RefreshCatalogAsync() => ReloadCatalogFromApiAsync();

    private void ApplyCategoryOptionsFromApi(List<CategoryDto> categories)
    {
        CategoryOptions.Clear();
        CategoryOptions.Add("All categories");
        foreach (var name in categories
            .Select(c => c.Name)
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(n => n, StringComparer.OrdinalIgnoreCase))
        {
            CategoryOptions.Add(name.Trim());
        }

        if (!CategoryOptions.Contains(SelectedCategory))
            SelectedCategory = CategoryOptions[0];
    }

    private async Task ReloadCatalogFromApiAsync()
    {
        App.RunOnUIThread(() =>
        {
            IsLoadingCatalog = true;
            CatalogLoadError = string.Empty;
        });
        try
        {
            var data = await FetchAllProductDtosAsync().ConfigureAwait(false);
            var list = data.ConvertAll(ProductDtoMapping.ToListItem);
            App.RunOnUIThread(() => ApplyCatalogFromApi(list));
        }
        catch (Exception ex)
        {
            App.RunOnUIThread(() => CatalogLoadError = ex.Message);
        }
        finally
        {
            App.RunOnUIThread(() => IsLoadingCatalog = false);
        }
    }

    /// <summary>Loads every page from the GraphQL <c>products</c> query so filters work on the full catalog.</summary>
    private async Task<List<ProductDto>> FetchAllProductDtosAsync(CancellationToken ct = default)
    {
        const int pageSize = 200;
        var sort = new ProductSortInput("CREATED_AT", "DESC");
        var first = await _productService
            .GetProductsAsync(1, pageSize, null, sort, ct)
            .ConfigureAwait(false);
        var acc = new List<ProductDto>(first.Data);
        for (var p = 2; p <= first.TotalPages; p++)
        {
            var next = await _productService
                .GetProductsAsync(p, pageSize, null, sort, ct)
                .ConfigureAwait(false);
            acc.AddRange(next.Data);
        }

        return acc;
    }

    private void ApplyCatalogFromApi(List<ProductListItem> list)
    {
        _catalog = list;
        if (CategoryOptions.Count <= 1)
            RebuildCategoryOptions();
        else
            MergeCategoryOptionsFromCatalog();
        UpdateStats();
        ApplyFilters();
    }

    private bool CanOpenCreateProductDialog() => IsReadyForCreateDialog && !IsCreateDialogOpen;

    [RelayCommand(CanExecute = nameof(CanOpenCreateProductDialog))]
    private async Task OpenCreateProductDialogAsync()
    {
        var vm = new CreateProductViewModel(
            _productService,
            _imageUpload,
            () =>
            {
                IsCreateDialogOpen = false;
                CreateDialogViewModel = null;
            },
            ReloadCatalogFromApiAsync);
        CreateDialogViewModel = vm;
        IsCreateDialogOpen = true;
        await vm.InitializeAsync();
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
            _ => q
                .OrderByDescending(p => p.ApiCreatedAt ?? DateTime.MinValue)
                .ThenBy(p => p.Name, StringComparer.OrdinalIgnoreCase),
        };

        _filtered = q.ToList();
        TotalPages = Math.Max(1, (int)Math.Ceiling(_filtered.Count / (double)PageSize));
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

        PageOptions.Clear();
        for (var i = 1; i <= TotalPages; i++)
            PageOptions.Add(i);
    }

    private void RebuildCurrentPage()
    {
        PagedItems.Clear();
        var skip = (CurrentPage - 1) * PageSize;
        foreach (var item in _filtered.Skip(skip).Take(PageSize))
            PagedItems.Add(item);

        RebuildPageButtons();

        OnPropertyChanged(nameof(PageSummaryText));
        OnPropertyChanged(nameof(CanGoPrevious));
        OnPropertyChanged(nameof(CanGoNext));
    }
}
