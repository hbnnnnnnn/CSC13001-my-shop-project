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

    public OrderService OrderServiceInstance => _orderService;

    public OrderListViewModel(OrderService orderService)
    {
        _orderService = orderService;
        SelectedStatusFilter = StatusFilterOptions[0];

        // Fire-and-forget load on construction
        _ = LoadOrdersAsync();
    }

    /// <summary>
    /// Fetches all orders from backend and applies client-side filters.
    /// </summary>
    public async Task LoadOrdersAsync()
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            Console.Error.WriteLine("[OrderListVM] Loading orders...");
            var (orders, _, _) = await _orderService.GetOrdersAsync(page: 1, limit: 500);
            Console.Error.WriteLine($"[OrderListVM] Got {orders.Count} orders");
            _allOrders = orders;
            ApplyFilters();
            Console.Error.WriteLine($"[OrderListVM] After filter: {FilteredOrders.Count} displayed");
        }
        catch (GraphqlException ex)
        {
            Console.Error.WriteLine($"[OrderListVM] GraphQL error: {ex.Message}");
            ErrorMessage = ex.Message;
        }
        catch (HttpRequestException ex)
        {
            Console.Error.WriteLine($"[OrderListVM] HTTP error: {ex.Message}");
            ErrorMessage = "Cannot connect to server. Check your connection.";
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[OrderListVM] Error: {ex.GetType().Name}: {ex.Message}");
            ErrorMessage = $"Failed to load orders: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    partial void OnSearchTextChanged(string value) => ApplyFilters();
    partial void OnSelectedStatusFilterChanged(string value) => ApplyFilters();
    partial void OnCurrentPageChanged(int value) => ApplyFilters();
    partial void OnPageSizeChanged(int value) { CurrentPage = 1; ApplyFilters(); }
    partial void OnFromDateChanged(DateTimeOffset? value) => ApplyFilters();
    partial void OnToDateChanged(DateTimeOffset? value) => ApplyFilters();

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
            _allOrders.Remove(order);
            ApplyFilters();
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

    private void ApplyFilters()
    {
        var query = _allOrders.AsEnumerable();

        // Search filter
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var search = SearchText.Trim().ToLowerInvariant();
            query = query.Where(o =>
                o.Id.ToLowerInvariant().Contains(search) ||
                o.CustomerName.ToLowerInvariant().Contains(search) ||
                o.Amount.ToLowerInvariant().Contains(search));
        }

        // Status filter
        if (!string.IsNullOrEmpty(SelectedStatusFilter) && SelectedStatusFilter != "All Status")
        {
            query = query.Where(o => o.Status == SelectedStatusFilter);
        }

        var filtered = query.ToList();
        TotalPages = Math.Max(1, (int)Math.Ceiling(filtered.Count / (double)PageSize));

        // Rebuild page options for ComboBox
        PageOptions.Clear();
        for (int i = 1; i <= TotalPages; i++)
            PageOptions.Add(i);

        if (CurrentPage > TotalPages)
            CurrentPage = TotalPages;

        var paged = filtered
            .Skip((CurrentPage - 1) * PageSize)
            .Take(PageSize)
            .ToList();

        FilteredOrders.Clear();
        foreach (var item in paged)
        {
            FilteredOrders.Add(item);
        }
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