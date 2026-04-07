using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CSC13001_my_shop_project.Services;

namespace CSC13001_my_shop_project.Presentation.OrderList;

public partial class CreateOrderViewModel : ObservableObject
{
    private readonly OrderService? _orderService;
    private readonly AuthService? _authService;

    [ObservableProperty]
    private string _dialogTitle = "CREATE ORDER";

    [ObservableProperty]
    private string _submitButtonText = "Create Order";

    [ObservableProperty]
    private string _customerName = string.Empty;

    [ObservableProperty]
    private string _phone = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _address = string.Empty;

    [ObservableProperty]
    private DateTimeOffset? _orderDate;

    [ObservableProperty]
    private string _selectedStatus = "Processing";

    [ObservableProperty]
    private bool _isSaving;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private ObservableCollection<OrderProductItem> _products = new();

    [ObservableProperty]
    private string _totalAmountFormatted = "0 ₫";

    // ─── Picker data ───

    [ObservableProperty]
    private ObservableCollection<ProductPickerItem> _availableProducts = new();

    [ObservableProperty]
    private ObservableCollection<CustomerPickerItem> _availableCustomers = new();

    [ObservableProperty]
    private CustomerPickerItem? _selectedCustomer;

    [ObservableProperty]
    private bool _isLoadingPickers;

    public Microsoft.UI.Xaml.Visibility HasNoProducts =>
        Products.Count == 0
            ? Microsoft.UI.Xaml.Visibility.Visible
            : Microsoft.UI.Xaml.Visibility.Collapsed;

    public List<string> StatusOptions { get; } = new()
    {
        "Created",
        "Processing",
        "Shipped",
        "Delivered",
        "Pending",
        "Cancelled"
    };

    /// <summary>
    /// Constructor for Create mode with API services.
    /// </summary>
    public CreateOrderViewModel(OrderService orderService, AuthService authService)
    {
        _orderService = orderService;
        _authService = authService;
        OrderDate = DateTimeOffset.Now;
        _ = LoadPickerDataAsync();
    }

    /// <summary>
    /// Constructor for Edit mode — pre-fill from an existing order.
    /// </summary>
    public CreateOrderViewModel(OrderItem order, OrderService orderService, AuthService authService)
    {
        _orderService = orderService;
        _authService = authService;

        DialogTitle = $"EDIT ORDER {order.Id}";
        SubmitButtonText = "Update Order";
        CustomerName = order.CustomerName;
        Phone = order.Phone;
        Email = order.Email;
        Address = order.Address;
        SelectedStatus = order.Status;

        // Parse date
        if (DateTimeOffset.TryParse(order.Date, out var parsed))
        {
            OrderDate = parsed;
        }
        else
        {
            OrderDate = DateTimeOffset.Now;
        }

        // Copy products
        Products = new ObservableCollection<OrderProductItem>(
            order.Products.Select(p => new OrderProductItem
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                Quantity = p.Quantity,
                UnitPrice = p.UnitPrice
            })
        );

        RefreshTotals();
        _ = LoadPickerDataAsync();
    }

    /// <summary>
    /// When a customer is selected from the picker, auto-fill their details.
    /// </summary>
    partial void OnSelectedCustomerChanged(CustomerPickerItem? value)
    {
        if (value is null) return;
        CustomerName = value.Name;
        Phone = value.Phone ?? "";
        Address = value.Address ?? "";
    }

    /// <summary>
    /// Load products and customers for picker ComboBoxes.
    /// </summary>
    public async Task LoadPickerDataAsync()
    {
        if (_orderService is null) return;
        IsLoadingPickers = true;

        try
        {
            var productsTask = _orderService.GetProductsForPickerAsync();
            var customersTask = _orderService.GetCustomersForPickerAsync();

            await Task.WhenAll(productsTask, customersTask);

            AvailableProducts = new ObservableCollection<ProductPickerItem>(productsTask.Result);
            AvailableCustomers = new ObservableCollection<CustomerPickerItem>(customersTask.Result);
        }
        catch
        {
            /* best-effort — pickers will be empty */
        }
        finally
        {
            IsLoadingPickers = false;
        }
    }

    public void RefreshTotals()
    {
        var total = Products.Sum(p => p.Total);
        TotalAmountFormatted = $"{total:N0} ₫";
        OnPropertyChanged(nameof(HasNoProducts));
    }

    public bool ValidateInput()
    {
        if (Products.Count == 0)
        {
            ErrorMessage = "Please add at least one product.";
            return false;
        }

        return true;
    }

    public async Task<bool> SaveAsync()
    {
        ErrorMessage = null;
        Debug.WriteLine("[CreateOrder] SaveAsync called");

        if (!ValidateInput())
        {
            Debug.WriteLine($"[CreateOrder] Validation failed: {ErrorMessage}");
            return false;
        }

        if (_orderService is null)
        {
            ErrorMessage = "Service not available.";
            Debug.WriteLine("[CreateOrder] OrderService is null");
            return false;
        }

        IsSaving = true;
        try
        {
            // Build items list: product_id + quantity
            var items = Products
                .Where(p => !string.IsNullOrEmpty(p.ProductId))
                .Select(p => (p.ProductId, p.Quantity))
                .ToList();

            Debug.WriteLine($"[CreateOrder] Items count: {items.Count}");
            foreach (var item in items)
            {
                Debug.WriteLine($"  - ProductId={item.ProductId}, Qty={item.Quantity}");
            }

            if (items.Count == 0)
            {
                ErrorMessage = "No valid products to submit.";
                Debug.WriteLine("[CreateOrder] No valid items");
                return false;
            }

            // Get customer_id from selected customer
            var customerId = SelectedCustomer?.CustomerId;
            Debug.WriteLine($"[CreateOrder] CustomerId={customerId}, Address={Address}");

            var result = await _orderService.CreateOrderAsync(
                customerId,
                string.IsNullOrWhiteSpace(Address) ? null : Address,
                items
            );

            Debug.WriteLine($"[CreateOrder] SUCCESS - Created order: {result.Id}");
            return true;
        }
        catch (GraphqlException ex)
        {
            ErrorMessage = ex.Message;
            Debug.WriteLine($"[CreateOrder] GraphQL error: {ex.Message}");
            return false;
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage = "Cannot connect to server.";
            Debug.WriteLine($"[CreateOrder] HTTP error: {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to create order: {ex.Message}";
            Debug.WriteLine($"[CreateOrder] Exception: {ex}");
            return false;
        }
        finally
        {
            IsSaving = false;
        }
    }
}
