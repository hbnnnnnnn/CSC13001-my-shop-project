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
        UpdateSelectedStatusColors();
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

    // ─── Status picker ───
    private void StatusOptionsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ListView lv && lv.SelectedItem is string status && DataContext is CreateOrderViewModel vm)
        {
            vm.SelectedStatus = status;
            UpdateSelectedStatusColors();

            // Close the flyout
            if (StatusPickerButton.Flyout is Flyout flyout)
            {
                flyout.Hide();
            }
        }
    }

    /// <summary>
    /// Applies the matching status color to the selected status dot + text,
    /// and shows the status picker panel when in edit mode.
    /// </summary>
    private void UpdateSelectedStatusColors()
    {
        if (DataContext is not CreateOrderViewModel vm) return;

        if (vm.IsEditMode)
        {
            StatusPickerPanel.Visibility = Visibility.Visible;
            Grid.SetColumn(DatePickerPanel, 1); // Move DATE next to STATUS

            var brush = GetStatusFgBrush(vm.SelectedStatus);
            SelectedStatusDot.Fill = brush;
            SelectedStatusText.Foreground = brush;

            // Apply colors to dropdown items when they load
            StatusOptionsList.ContainerContentChanging -= OnStatusItemContentChanging;
            StatusOptionsList.ContainerContentChanging += OnStatusItemContentChanging;

            // Enable/disable fields based on whether target status allows editing
            UpdateFieldsEditability(vm.SelectedStatus);
        }
    }

    /// <summary>
    /// Enables or disables input fields based on the selected target status.
    /// Only Created and Processing allow editing customer info, products, and address.
    /// Shipped/Delivered/Cancelled are status-only transitions.
    /// </summary>
    private void UpdateFieldsEditability(string targetStatus)
    {
        var isEditable = targetStatus is "Created" or "Processing";

        // Customer info fields
        CustomerPicker.IsEnabled = isEditable;
        PhoneTextBox.IsReadOnly = !isEditable;
        EmailTextBox.IsReadOnly = !isEditable;
        AddressTextBox.IsReadOnly = !isEditable;

        // Date picker
        DatePickerButton.IsEnabled = isEditable;

        // Product add button
        AddProductButton.IsEnabled = isEditable;

        // Visual feedback: dim the locked sections
        CustomerDetailsPanel.Opacity = isEditable ? 1.0 : 0.5;
        DatePickerPanel.Opacity = isEditable ? 1.0 : 0.5;
    }

    /// <summary>
    /// Colors each dropdown item's dot + text with the matching status color.
    /// </summary>
    private void OnStatusItemContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
    {
        if (args.Item is string status && args.ItemContainer?.ContentTemplateRoot is StackPanel panel)
        {
            var brush = GetStatusFgBrush(status);
            foreach (var child in panel.Children)
            {
                if (child is Microsoft.UI.Xaml.Shapes.Ellipse dot)
                    dot.Fill = brush;
                else if (child is TextBlock label)
                    label.Foreground = brush;
            }
        }
    }

    /// <summary>
    /// Resolves the foreground brush for a given status from App.xaml resources.
    /// </summary>
    private static SolidColorBrush GetStatusFgBrush(string status)
    {
        var key = status switch
        {
            "Created" => "StatusCreatedFgBrush",
            "Processing" => "StatusProcessingFgBrush",
            "Shipped" => "StatusShippedFgBrush",
            "Delivered" => "StatusDeliveredFgBrush",
            "Cancelled" => "StatusCancelledFgBrush",
            _ => "StatusCreatedFgBrush"
        };

        if (Application.Current.Resources.TryGetValue(key, out var resource) && resource is SolidColorBrush brush)
        {
            return brush;
        }

        return new SolidColorBrush(Microsoft.UI.Colors.Gray);
    }
}
