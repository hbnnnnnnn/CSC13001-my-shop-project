namespace CSC13001_my_shop_project.Presentation.OrderList;

using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
using Windows.UI;

public partial class OrderListViewModel : ObservableObject
{
    private readonly List<OrderItem> _allOrders;

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

    public List<string> StatusFilterOptions { get; } = new()
    {
        "All Status",
        "Processing",
        "Shipped",
        "Delivered",
        "Pending",
        "Cancelled"
    };

    public OrderListViewModel()
    {
        SelectedStatusFilter = StatusFilterOptions[0];

        _allOrders = new List<OrderItem>
        {
            new()
            {
                Id = "#00123", Date = "03/02/2026", CustomerName = "Eleanor Pena", Status = "Processing", Amount = "$450.00",
                Phone = "+1 (555) 012-3456", Email = "eleanor@example.com", Address = "789 Oak Lane\nChicago, IL 60601, USA",
                Products = new ObservableCollection<OrderProductItem>
                {
                    new() { ProductName = "Nordic Lounge Chair", Quantity = 2, UnitPrice = 124.50m },
                    new() { ProductName = "Minimalist Wooden Lamp", Quantity = 1, UnitPrice = 89.00m },
                    new() { ProductName = "Ceramic Artisan Vase", Quantity = 3, UnitPrice = 37.00m }
                },
                ShippingFee = 25.00m
            },
            new()
            {
                Id = "#00124", Date = "03/02/2026", CustomerName = "Wade Warren", Status = "Shipped", Amount = "$1,248.00",
                Phone = "+1 (555) 234-5678", Email = "wade@example.com", Address = "456 Elm Street\nLos Angeles, CA 90001, USA",
                Products = new ObservableCollection<OrderProductItem>
                {
                    new() { ProductName = "Designer Coffee Table", Quantity = 1, UnitPrice = 899.00m },
                    new() { ProductName = "Artisan Throw Pillow", Quantity = 4, UnitPrice = 62.25m }
                },
                ShippingFee = 30.00m
            },
            new()
            {
                Id = "#00125", Date = "03/01/2026", CustomerName = "Esther Howard", Status = "Delivered", Amount = "$2,450.00",
                Phone = "+1 (555) 345-6789", Email = "esther@example.com", Address = "321 Pine Avenue\nNew York, NY 10001, USA",
                Products = new ObservableCollection<OrderProductItem>
                {
                    new() { ProductName = "Premium Leather Sofa", Quantity = 1, UnitPrice = 2400.00m }
                },
                ShippingFee = 50.00m
            },
            new()
            {
                Id = "#00126", Date = "02/28/2026", CustomerName = "Cameron Williamson", Status = "Pending", Amount = "$854.00",
                Phone = "+1 (555) 456-7890", Email = "cameron@example.com", Address = "654 Maple Drive\nSan Francisco, CA 94102, USA",
                Products = new ObservableCollection<OrderProductItem>
                {
                    new() { ProductName = "Scandinavian Dining Chair", Quantity = 4, UnitPrice = 199.00m },
                    new() { ProductName = "Linen Table Runner", Quantity = 2, UnitPrice = 29.00m }
                },
                ShippingFee = 0m
            },
            new()
            {
                Id = "#00127", Date = "02/28/2026", CustomerName = "Brooklyn Simmons", Status = "Processing", Amount = "$320.00",
                Phone = "+1 (555) 567-8901", Email = "brooklyn@example.com", Address = "987 Cedar Blvd\nSeattle, WA 98101, USA",
                Products = new ObservableCollection<OrderProductItem>
                {
                    new() { ProductName = "Woven Basket Set", Quantity = 2, UnitPrice = 85.00m },
                    new() { ProductName = "Concrete Planter", Quantity = 3, UnitPrice = 45.00m }
                },
                ShippingFee = 15.00m
            },
            new()
            {
                Id = "#00128", Date = "02/27/2026", CustomerName = "Leslie Alexander", Status = "Cancelled", Amount = "$120.00",
                Phone = "+1 (555) 678-9012", Email = "leslie@example.com", Address = "147 Birch Court\nAustin, TX 73301, USA",
                Products = new ObservableCollection<OrderProductItem>
                {
                    new() { ProductName = "Ceramic Artisan Vase", Quantity = 2, UnitPrice = 37.00m },
                    new() { ProductName = "Scented Soy Candle", Quantity = 3, UnitPrice = 15.33m }
                },
                ShippingFee = 0m
            },
            new()
            {
                Id = "#00129", Date = "02/26/2026", CustomerName = "Jenny Wilson", Status = "Delivered", Amount = "$2,890.00",
                Phone = "+1 (555) 789-0123", Email = "jenny@example.com", Address = "258 Walnut Way\nDenver, CO 80202, USA",
                Products = new ObservableCollection<OrderProductItem>
                {
                    new() { ProductName = "Minimalist Bookshelf", Quantity = 2, UnitPrice = 650.00m },
                    new() { ProductName = "Brass Table Lamp", Quantity = 1, UnitPrice = 320.00m },
                    new() { ProductName = "Handwoven Rug", Quantity = 1, UnitPrice = 1200.00m }
                },
                ShippingFee = 70.00m
            },
            new()
            {
                Id = "#00130", Date = "02/25/2026", CustomerName = "Guy Hawkins", Status = "Shipped", Amount = "$560.00",
                Phone = "+1 (555) 890-1234", Email = "guy@example.com", Address = "369 Spruce Street\nPortland, OR 97201, USA",
                Products = new ObservableCollection<OrderProductItem>
                {
                    new() { ProductName = "Nordic Lounge Chair", Quantity = 1, UnitPrice = 510.00m }
                },
                ShippingFee = 50.00m
            },
            new()
            {
                Id = "#00131", Date = "02/24/2026", CustomerName = "Robert Fox", Status = "Processing", Amount = "$1,100.00",
                Phone = "+1 (555) 901-2345", Email = "robert@example.com", Address = "741 Ash Lane\nMiami, FL 33101, USA",
                Products = new ObservableCollection<OrderProductItem>
                {
                    new() { ProductName = "Designer Coffee Table", Quantity = 1, UnitPrice = 899.00m },
                    new() { ProductName = "Minimalist Wooden Lamp", Quantity = 2, UnitPrice = 89.00m }
                },
                ShippingFee = 23.00m
            },
            new()
            {
                Id = "#00132", Date = "02/23/2026", CustomerName = "Jacob Jones", Status = "Delivered", Amount = "$3,200.00",
                Phone = "+1 (555) 012-3456", Email = "customer@example.com", Address = "1234 Design Avenue, Suite 500\nNew York, NY 10001, USA",
                Products = new ObservableCollection<OrderProductItem>
                {
                    new() { ProductName = "Nordic Lounge Chair", Quantity = 2, UnitPrice = 124.50m },
                    new() { ProductName = "Minimalist Wooden Lamp", Quantity = 1, UnitPrice = 89.00m },
                    new() { ProductName = "Ceramic Artisan Vase", Quantity = 3, UnitPrice = 37.00m }
                },
                ShippingFee = 25.00m
            }
        };

        ApplyFilters();
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
    private string _productName = "";

    [ObservableProperty]
    private int _quantity;

    [ObservableProperty]
    private decimal _unitPrice;

    public decimal Total => Quantity * UnitPrice;

    public string UnitPriceFormatted => $"${UnitPrice:N2}";
    public string TotalFormatted => $"${Total:N2}";

    partial void OnQuantityChanged(int value) => OnPropertyChanged(nameof(Total));
    partial void OnUnitPriceChanged(decimal value) => OnPropertyChanged(nameof(Total));
}

public partial class OrderItem : ObservableObject
{
    [ObservableProperty]
    private string _id = "";
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
    public string SubtotalFormatted => $"${Subtotal:N2}";
    public string ShippingFeeFormatted => $"${ShippingFee:N2}";
    public string TotalAmountFormatted => $"${TotalAmount:N2}";

    public SolidColorBrush StatusBgBrush => GetStatusBrush("Bg");
    public SolidColorBrush StatusFgBrush => GetStatusBrush("Fg");
    public SolidColorBrush StatusBorderBrush => GetStatusBrush("Border");

    /// <summary>
    /// Returns the prominent accent color for dot+text display.
    /// For filled badges (Processing, Pending) → BgBrush.
    /// For outlined badges (Shipped, Delivered, Cancelled) → FgBrush.
    /// </summary>
    public SolidColorBrush StatusAccentBrush => Status switch
    {
        "Processing" => GetStatusBrush("Bg"),
        "Pending" => GetStatusBrush("Bg"),
        _ => GetStatusBrush("Fg")
    };

    private SolidColorBrush GetStatusBrush(string part)
    {
        var key = Status switch
        {
            "Processing" => $"StatusProcessingBrush",
            "Shipped" => $"StatusShippedBrush",
            "Delivered" => $"StatusDeliveredBrush",
            "Pending" => $"StatusPendingBrush",
            "Cancelled" => $"StatusCancelledBrush",
            _ => $"StatusShippedBrush"
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
    }
}