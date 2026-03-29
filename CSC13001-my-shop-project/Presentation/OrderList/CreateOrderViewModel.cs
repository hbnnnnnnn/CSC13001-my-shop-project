using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CSC13001_my_shop_project.Presentation.OrderList;

public partial class CreateOrderViewModel : ObservableObject
{
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
    private ObservableCollection<OrderProductItem> _products = new();

    [ObservableProperty]
    private string _totalAmountFormatted = "$0.00";

    public Microsoft.UI.Xaml.Visibility HasNoProducts =>
        Products.Count == 0
            ? Microsoft.UI.Xaml.Visibility.Visible
            : Microsoft.UI.Xaml.Visibility.Collapsed;

    public List<string> StatusOptions { get; } = new()
    {
        "Processing",
        "Shipped",
        "Delivered",
        "Pending",
        "Cancelled"
    };

    public CreateOrderViewModel()
    {
        OrderDate = DateTimeOffset.Now;
    }

    /// <summary>
    /// Constructor for Edit mode — pre-fill from an existing order.
    /// </summary>
    public CreateOrderViewModel(OrderItem order)
    {
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
                ProductName = p.ProductName,
                Quantity = p.Quantity,
                UnitPrice = p.UnitPrice
            })
        );

        RefreshTotals();
    }

    public void RefreshTotals()
    {
        var total = Products.Sum(p => p.Total);
        TotalAmountFormatted = $"${total:N2}";
        OnPropertyChanged(nameof(HasNoProducts));
    }

    public bool ValidateInput()
    {
        return !string.IsNullOrWhiteSpace(CustomerName);
    }

    public async Task<bool> SaveAsync()
    {
        if (!ValidateInput())
        {
            return false;
        }

        IsSaving = true;
        try
        {
            await Task.CompletedTask;
            return true;
        }
        finally
        {
            IsSaving = false;
        }
    }
}
