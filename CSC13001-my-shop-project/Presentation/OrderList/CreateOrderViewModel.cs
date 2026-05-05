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
    private readonly string? _editOrderId;

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

    public List<string> StatusOptions { get; set; } = new()
    {
        "Created",
        "Processing",
        "Shipped",
        "Delivered",
        "Cancelled"
    };

    public bool IsEditMode => _editOrderId is not null;

    /// <summary>
    /// True when fields (info, products, date) can be edited.
    /// Only Created and Processing allow full editing.
    /// </summary>
    public bool IsFieldsEditable => SelectedStatus is "Created" or "Processing";

    /// <summary>
    /// Visibility for Remove buttons in the product list DataTemplate.
    /// </summary>
    public Microsoft.UI.Xaml.Visibility RemoveButtonVisibility =>
        IsFieldsEditable
            ? Microsoft.UI.Xaml.Visibility.Visible
            : Microsoft.UI.Xaml.Visibility.Collapsed;

    /// <summary>
    /// When status changes, notify dependent computed properties.
    /// </summary>
    partial void OnSelectedStatusChanged(string value)
    {
        OnPropertyChanged(nameof(IsFieldsEditable));
        OnPropertyChanged(nameof(RemoveButtonVisibility));
    }

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
        _editOrderId = order.Id.TrimStart('#');

        DialogTitle = $"EDIT ORDER {order.Id}";
        SubmitButtonText = "Update Order";
        CustomerName = order.CustomerName;
        Phone = order.Phone;
        Email = order.Email;
        Address = order.Address;
        SelectedStatus = order.Status;

        // Restrict status options based on state machine
        StatusOptions = GetAllowedStatuses(order.Status);

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
    /// Returns the list of statuses reachable from the current status.
    /// Follows backend state machine transitions.
    /// </summary>
    private static List<string> GetAllowedStatuses(string currentStatus)
    {
        var transitions = new Dictionary<string, List<string>>
        {
            ["Created"] = ["Created", "Processing", "Cancelled"],
            ["Processing"] = ["Processing", "Shipped", "Cancelled"],
            ["Shipped"] = ["Shipped", "Delivered"],
            ["Delivered"] = ["Delivered"],
            ["Cancelled"] = ["Cancelled"]
        };

        return transitions.TryGetValue(currentStatus, out var allowed)
            ? allowed
            : [currentStatus];
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
        Debug.WriteLine($"[CreateOrder] SaveAsync called (IsEditMode={IsEditMode})");

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

            if (items.Count == 0)
            {
                ErrorMessage = "No valid products to submit.";
                return false;
            }

            if (IsEditMode)
            {
                // ── Edit mode: call updateOrderFull ──
                Debug.WriteLine($"[CreateOrder] Updating order {_editOrderId}");

                // Backend blocks items/info edits when target status is Shipped or Delivered.
                // Only send status in that case, send full payload otherwise.
                var isStatusOnly = SelectedStatus is "Shipped" or "Delivered" or "Cancelled";

                if (isStatusOnly)
                {
                    Debug.WriteLine($"[CreateOrder] Status-only update → {SelectedStatus}");
                    var result = await _orderService.UpdateOrderFullAsync(
                        _editOrderId!,
                        status: SelectedStatus
                    );
                    Debug.WriteLine($"[CreateOrder] SUCCESS - Updated order: {result.Id}");
                }
                else
                {
                    var result = await _orderService.UpdateOrderFullAsync(
                        _editOrderId!,
                        status: SelectedStatus,
                        shippingAddress: string.IsNullOrWhiteSpace(Address) ? null : Address,
                        recipientName: string.IsNullOrWhiteSpace(CustomerName) ? null : CustomerName,
                        recipientPhone: string.IsNullOrWhiteSpace(Phone) ? null : Phone,
                        recipientEmail: string.IsNullOrWhiteSpace(Email) ? null : Email,
                        items: items
                    );
                    Debug.WriteLine($"[CreateOrder] SUCCESS - Updated order: {result.Id}");
                }
            }
            else
            {
                // ── Create mode: call createOrder ──
                var customerId = SelectedCustomer?.CustomerId;
                Debug.WriteLine($"[CreateOrder] Creating order, CustomerId={customerId}");

                var result = await _orderService.CreateOrderAsync(
                    customerId,
                    string.IsNullOrWhiteSpace(Address) ? null : Address,
                    items
                );
                Debug.WriteLine($"[CreateOrder] SUCCESS - Created order: {result.Id}");
            }

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
            ErrorMessage = $"Failed to save order: {ex.Message}";
            Debug.WriteLine($"[CreateOrder] Exception: {ex}");
            return false;
        }
        finally
        {
            IsSaving = false;
        }
    }
}
