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
        _allOrders = new List<OrderItem>
        {
            new() { Id = "#00123", Date = "03/02/2026", CustomerName = "Eleanor Pena", Status = "Processing", Amount = "$450.00" },
            new() { Id = "#00124", Date = "03/02/2026", CustomerName = "Wade Warren", Status = "Shipped", Amount = "$1,248.00" },
            new() { Id = "#00125", Date = "03/01/2026", CustomerName = "Esther Howard", Status = "Delivered", Amount = "$2,450.00" },
            new() { Id = "#00126", Date = "02/28/2026", CustomerName = "Cameron Williamson", Status = "Pending", Amount = "$854.00" },
            new() { Id = "#00127", Date = "02/28/2026", CustomerName = "Brooklyn Simmons", Status = "Processing", Amount = "$320.00" },
            new() { Id = "#00128", Date = "02/27/2026", CustomerName = "Leslie Alexander", Status = "Cancelled", Amount = "$120.00" },
            new() { Id = "#00129", Date = "02/26/2026", CustomerName = "Jenny Wilson", Status = "Delivered", Amount = "$2,890.00" },
            new() { Id = "#00130", Date = "02/25/2026", CustomerName = "Guy Hawkins", Status = "Shipped", Amount = "$560.00" },
            new() { Id = "#00131", Date = "02/24/2026", CustomerName = "Robert Fox", Status = "Processing", Amount = "$1,100.00" },
            new() { Id = "#00132", Date = "02/23/2026", CustomerName = "Jacob Jones", Status = "Delivered", Amount = "$3,200.00" }
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

    public SolidColorBrush StatusBgBrush => GetStatusBrush("Bg");
    public SolidColorBrush StatusFgBrush => GetStatusBrush("Fg");
    public SolidColorBrush StatusBorderBrush => GetStatusBrush("Border");

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