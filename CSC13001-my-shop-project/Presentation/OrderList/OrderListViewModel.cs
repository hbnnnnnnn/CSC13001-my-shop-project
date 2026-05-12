namespace CSC13001_my_shop_project.Presentation.OrderList;

using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CSC13001_my_shop_project.Services;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
using Windows.UI;

public partial class OrderListViewModel : ObservableObject
{
    private readonly OrderService _orderService;
    private List<OrderItem> _allOrders = [];
    private bool _isLoadingInProgress;

    [ObservableProperty]
    private ObservableCollection<OrderItem> _filteredOrders = new();

    [ObservableProperty]
    private string _searchText = "";

    [ObservableProperty]
    private string _selectedStatusFilter = "All Status";

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _pageSize = 10;

    [ObservableProperty]
    private int _totalPages = 1;

    [ObservableProperty]
    private ObservableCollection<int> _pageOptions = new() { 1 };

    [ObservableProperty]
    private DateTimeOffset? _fromDate;

    [ObservableProperty]
    private DateTimeOffset? _toDate;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    public List<string> StatusFilterOptions { get; } = new()
    {
        "All Status",
        "Created",
        "Processing",
        "Shipped",
        "Delivered",
        "Cancelled"
    };

    // ── Empty state computed properties ─────────────────────────────────
    public bool HasOrders => FilteredOrders.Count > 0;
    public bool HasActiveFilters => !string.IsNullOrEmpty(SearchText)
        || SelectedStatusFilter != "All Status"
        || FromDate != null || ToDate != null;
    public bool ShowEmptyState => !IsLoading && !HasOrders;
    public bool ShowNoResults => ShowEmptyState && HasActiveFilters;
    public bool ShowFirstTimeEmpty => ShowEmptyState && !HasActiveFilters;
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    public OrderService OrderServiceInstance => _orderService;

    public OrderListViewModel(OrderService orderService)
    {
        _orderService = orderService;
        SelectedStatusFilter = StatusFilterOptions[0];

        // Fire-and-forget load on construction
        _ = LoadOrdersAsync();
    }

    /// <summary>
    /// Fetches orders from backend with server-side filters and pagination.
    /// </summary>
    public async Task LoadOrdersAsync()
    {
        // Prevent re-entrant calls (filter changes can cascade)
        if (_isLoadingInProgress) return;
        _isLoadingInProgress = true;

        App.RunOnUIThread(() =>
        {
            IsLoading = true;
            ErrorMessage = null;
            OnPropertyChanged(nameof(HasError));
        });

        try
        {
            Console.Error.WriteLine("[OrderListVM] Loading orders...");

            // Build server-side filter params
            string? statusFilter = (SelectedStatusFilter != "All Status")
                ? SelectedStatusFilter
                : null;

            string? startDate = FromDate?.UtcDateTime.ToString("o");
            string? endDate = ToDate?.UtcDateTime.Date.AddDays(1).AddTicks(-1).ToString("o");

            var (orders, total, totalPages) = await _orderService.GetOrdersAsync(
                page: CurrentPage,
                limit: PageSize,
                statusFilter: statusFilter,
                startDate: startDate,
                endDate: endDate,
                sortField: "CREATED_TIME",
                sortOrder: "DESC"
            );

            Console.Error.WriteLine($"[OrderListVM] Got {orders.Count} orders (total: {total})");

            _allOrders = orders;

            // Build new page options list
            var newPageOptions = new ObservableCollection<int>();
            var newTotalPages = Math.Max(1, totalPages);
            for (int i = 1; i <= newTotalPages; i++)
                newPageOptions.Add(i);

            // Assign on UI thread
            App.RunOnUIThread(() =>
            {
                TotalPages = newTotalPages;
                PageOptions = newPageOptions;
            });

            // Client-side search only (backend has no full-text search for orders)
            ApplyClientSearch();
        }
        catch (GraphqlException ex)
        {
            Console.Error.WriteLine($"[OrderListVM] GraphQL error: {ex.Message}");
            App.RunOnUIThread(() => { ErrorMessage = ex.Message; OnPropertyChanged(nameof(HasError)); });
        }
        catch (HttpRequestException ex)
        {
            Console.Error.WriteLine($"[OrderListVM] HTTP error: {ex.Message}");
            App.RunOnUIThread(() => { ErrorMessage = "Cannot connect to server. Check your connection."; OnPropertyChanged(nameof(HasError)); });
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[OrderListVM] Error: {ex.GetType().Name}: {ex.Message}");
            App.RunOnUIThread(() => { ErrorMessage = $"Failed to load orders: {ex.Message}"; OnPropertyChanged(nameof(HasError)); });
        }
        finally
        {
            App.RunOnUIThread(() => IsLoading = false);
            _isLoadingInProgress = false;
        }
    }

    // Status/date filter changes → reload from backend (reset to page 1)
    partial void OnSelectedStatusFilterChanged(string value) { CurrentPage = 1; _ = LoadOrdersAsync(); }
    partial void OnFromDateChanged(DateTimeOffset? value) { CurrentPage = 1; _ = LoadOrdersAsync(); }
    partial void OnToDateChanged(DateTimeOffset? value) { CurrentPage = 1; _ = LoadOrdersAsync(); }

    // Pagination changes → reload from backend
    partial void OnCurrentPageChanged(int value) => _ = LoadOrdersAsync();
    partial void OnPageSizeChanged(int value) { CurrentPage = 1; _ = LoadOrdersAsync(); }

    // Search text → client-side only (no backend full-text search for orders)
    partial void OnSearchTextChanged(string value) => ApplyClientSearch();

    [RelayCommand]
    private void GoToPage(int page)
    {
        if (page >= 1 && page <= TotalPages)
        {
            CurrentPage = page;
        }
    }

    [RelayCommand]
    private void PreviousPage()
    {
        if (CurrentPage > 1)
            CurrentPage--;
    }

    [RelayCommand]
    private void NextPage()
    {
        if (CurrentPage < TotalPages)
            CurrentPage++;
    }

    /// <summary>
    /// Deletes an order via the backend (soft delete) and refreshes the list.
    /// </summary>
    public async Task<bool> DeleteOrderAsync(OrderItem order)
    {
        try
        {
            var rawId = order.Id.TrimStart('#');
            await _orderService.DeleteOrderAsync(rawId);
            await LoadOrdersAsync(); // Refresh from server
            return true;
        }
        catch (GraphqlException ex)
        {
            ErrorMessage = ex.Message;
            return false;
        }
        catch (HttpRequestException)
        {
            ErrorMessage = "Cannot connect to server.";
            return false;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to delete order: {ex.Message}";
            return false;
        }
    }

    /// <summary>
    /// Client-side search within the already-fetched page of orders.
    /// </summary>
    private void ApplyClientSearch()
    {
        var query = _allOrders.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var search = SearchText.Trim().ToLowerInvariant();
            query = query.Where(o =>
                o.Id.ToLowerInvariant().Contains(search) ||
                o.CustomerName.ToLowerInvariant().Contains(search) ||
                o.Amount.ToLowerInvariant().Contains(search));
        }

        var items = new ObservableCollection<OrderItem>(query);
        App.RunOnUIThread(() =>
        {
            FilteredOrders = items;
            OnPropertyChanged(nameof(HasOrders));
            OnPropertyChanged(nameof(ShowEmptyState));
            OnPropertyChanged(nameof(ShowNoResults));
            OnPropertyChanged(nameof(ShowFirstTimeEmpty));
        });
    }
}

public partial class OrderProductItem : ObservableObject
{
    [ObservableProperty]
    private string _productId = "";

    [ObservableProperty]
    private string _productName = "";

    [ObservableProperty]
    private int _quantity;

    [ObservableProperty]
    private decimal _unitPrice;

    public decimal Total => Quantity * UnitPrice;

    public string UnitPriceFormatted => $"{UnitPrice:N0} ₫";
    public string TotalFormatted => $"{Total:N0} ₫";

    partial void OnQuantityChanged(int value)
    {
        OnPropertyChanged(nameof(Total));
        OnPropertyChanged(nameof(TotalFormatted));
    }

    partial void OnUnitPriceChanged(decimal value)
    {
        OnPropertyChanged(nameof(Total));
        OnPropertyChanged(nameof(UnitPriceFormatted));
        OnPropertyChanged(nameof(TotalFormatted));
    }
}

public partial class OrderItem : ObservableObject
{
    [ObservableProperty]
    private string _id = "";
    [ObservableProperty]
    private string _customerId = "";
    [ObservableProperty]
    private string _date = "";
    [ObservableProperty]
    private string _customerName = "";
    [ObservableProperty]
    private string _status = "";
    [ObservableProperty]
    private string _amount = "";

    [ObservableProperty]
    private string _phone = "";
    [ObservableProperty]
    private string _email = "";
    [ObservableProperty]
    private string _address = "";
    [ObservableProperty]
    private decimal _shippingFee;
    [ObservableProperty]
    private ObservableCollection<OrderProductItem> _products = new();

    public decimal Subtotal => Products?.Sum(p => p.Total) ?? 0m;
    public decimal TotalAmount => Subtotal + ShippingFee;
    public string SubtotalFormatted => $"{Subtotal:N0} ₫";
    public string ShippingFeeFormatted => $"{ShippingFee:N0} ₫";
    public string TotalAmountFormatted => $"{TotalAmount:N0} ₫";

    public SolidColorBrush StatusBgBrush => GetStatusBrush("Bg");
    public SolidColorBrush StatusFgBrush => GetStatusBrush("Fg");
    public SolidColorBrush StatusBorderBrush => GetStatusBrush("Border");

    /// <summary>
    /// Returns the prominent accent color for dot+text display in detail view.
    /// All statuses now use outline style → FgBrush is the accent color.
    /// </summary>
    public SolidColorBrush StatusAccentBrush => GetStatusBrush("Fg");

    private SolidColorBrush GetStatusBrush(string part)
    {
        var key = Status switch
        {
            "Created" => "StatusCreatedBrush",
            "Processing" => "StatusProcessingBrush",
            "Shipped" => "StatusShippedBrush",
            "Delivered" => "StatusDeliveredBrush",
            "Pending" => "StatusPendingBrush",
            "Cancelled" => "StatusCancelledBrush",
            _ => "StatusShippedBrush"
        };

        // Replace "Brush" with the correct part
        key = key.Replace("Brush", $"{part}Brush");

        if (Application.Current.Resources.TryGetValue(key, out var resource) && resource is SolidColorBrush brush)
        {
            return brush;
        }

        return new SolidColorBrush(Colors.Gray);
    }

    partial void OnStatusChanged(string value)
    {
        OnPropertyChanged(nameof(StatusBgBrush));
        OnPropertyChanged(nameof(StatusFgBrush));
        OnPropertyChanged(nameof(StatusBorderBrush));
        OnPropertyChanged(nameof(StatusAccentBrush));
    }
}