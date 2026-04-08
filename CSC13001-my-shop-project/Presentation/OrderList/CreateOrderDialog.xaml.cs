using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace CSC13001_my_shop_project.Presentation.OrderList;

public sealed partial class CreateOrderDialog : ContentDialog
{
    /// <summary>
    /// Set to true when order was created/updated successfully.
    /// </summary>
    public bool IsSuccess { get; private set; }

    /// <summary>
    /// Completes when the dialog is done (success or cancel).
    /// </summary>
    private readonly TaskCompletionSource<bool> _completionSource = new();
    public Task<bool> WaitForResultAsync() => _completionSource.Task;

    public CreateOrderDialog()
    {
        this.InitializeComponent();
        this.Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        UpdateEmptyState();
        UpdateDateDisplay();
    }

    private void UpdateEmptyState()
    {
        if (DataContext is CreateOrderViewModel vm)
        {
            EmptyProductsPanel.Visibility = vm.Products.Count == 0
                ? Visibility.Visible
                : Visibility.Collapsed;
        }
    }

    private void UpdateDateDisplay()
    {
        if (DataContext is CreateOrderViewModel vm && vm.OrderDate.HasValue)
        {
            DateDisplayText.Text = vm.OrderDate.Value.ToString("MM/dd/yyyy");
        }
        else
        {
            DateDisplayText.Text = "Select date";
        }
    }

    // ─── Header buttons ───
    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        IsSuccess = false;
        _completionSource.TrySetResult(false);
        this.Hide();
    }

    private async void SubmitButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is CreateOrderViewModel vm)
        {
            var saved = await vm.SaveAsync();
            if (saved)
            {
                IsSuccess = true;
                _completionSource.TrySetResult(true);
                this.Hide();
            }
            else if (!string.IsNullOrEmpty(vm.ErrorMessage))
            {
                // Show error in a dialog
                var errorDialog = new ContentDialog
                {
                    XamlRoot = this.XamlRoot,
                    Title = "Error",
                    Content = vm.ErrorMessage,
                    CloseButtonText = "OK"
                };
                this.Hide();
                await errorDialog.ShowAsync();
                await this.ShowAsync();
            }
        }
    }

    // ─── DatePickerFlyout ───
    private void DatePickerFlyout_DatePicked(DatePickerFlyout sender, DatePickedEventArgs args)
    {
        if (DataContext is CreateOrderViewModel vm)
        {
            vm.OrderDate = args.NewDate;
            DateDisplayText.Text = args.NewDate.ToString("MM/dd/yyyy");
        }
    }

    // ─── Add product sub-dialog ───
    private async void AddProduct_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not CreateOrderViewModel vm)
            return;

        // Must hide parent dialog first (WinUI/Uno allows only one ContentDialog at a time)
        this.Hide();

        var addDialog = new ContentDialog
        {
            XamlRoot = this.XamlRoot,
            Title = "Add Product",
            PrimaryButtonText = "Add",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary
        };

        var panel = new StackPanel { Spacing = 12 };

        // Product picker ComboBox (loaded from backend)
        var productCombo = new ComboBox
        {
            Header = "Product",
            PlaceholderText = "Select a product...",
            HorizontalAlignment = HorizontalAlignment.Stretch,
            DisplayMemberPath = "DisplayText",
            ItemsSource = vm.AvailableProducts
        };

        var qtyBox = new NumberBox
        {
            PlaceholderText = "1",
            Header = "Quantity",
            Minimum = 1,
            Value = 1,
            SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Inline
        };

        var priceText = new TextBlock
        {
            Text = "Unit price: —",
            FontSize = 14,
            Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(0x99, 0x2C, 0x21, 0x18)),
            Margin = new Thickness(0, 4, 0, 0)
        };

        // Auto-fill price when product is selected
        productCombo.SelectionChanged += (_, _) =>
        {
            if (productCombo.SelectedItem is Services.ProductPickerItem selected)
            {
                priceText.Text = $"Đơn giá: {selected.Price:N0} ₫  |  Tồn kho: {selected.Stock}";
            }
        };

        panel.Children.Add(productCombo);
        panel.Children.Add(qtyBox);
        panel.Children.Add(priceText);
        addDialog.Content = panel;

        var result = await addDialog.ShowAsync();
        if (result == ContentDialogResult.Primary && productCombo.SelectedItem is Services.ProductPickerItem selectedProduct)
        {
            var qty = (int)qtyBox.Value;

            // Check if product already exists — merge quantity if so
            var existing = vm.Products.FirstOrDefault(p => p.ProductId == selectedProduct.ProductId);
            if (existing != null)
            {
                existing.Quantity += qty;
            }
            else
            {
                vm.Products.Add(new OrderProductItem
                {
                    ProductId = selectedProduct.ProductId,
                    ProductName = selectedProduct.Name,
                    Quantity = qty,
                    UnitPrice = selectedProduct.Price,
                });
            }
            vm.RefreshTotals();
        }

        // Re-show this dialog
        UpdateEmptyState();
        await this.ShowAsync();
    }

    // ─── Remove product ───
    private void RemoveProduct_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is OrderProductItem product && DataContext is CreateOrderViewModel vm)
        {
            vm.Products.Remove(product);
            vm.RefreshTotals();
            UpdateEmptyState();
        }
    }
}
