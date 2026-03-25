using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;

namespace CSC13001_my_shop_project.Presentation.OrderList;

public sealed partial class CreateOrderDialog : ContentDialog
{
    public CreateOrderDialog()
    {
        this.InitializeComponent();
        this.Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        UpdateEmptyState();
        UpdateDateDisplay();
        UpdateStatusDisplay();
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

    private void UpdateStatusDisplay()
    {
        if (DataContext is CreateOrderViewModel vm && !string.IsNullOrEmpty(vm.SelectedStatus))
        {
            StatusFilterText.Text = vm.SelectedStatus;
            ApplyStatusColor(vm.SelectedStatus);
        }
    }

    /// <summary>
    /// Apply status-matching color to the button dot and text.
    /// </summary>
    private void ApplyStatusColor(string status)
    {
        var brush = GetStatusAccentBrush(status);
        StatusDotBtn.Fill = brush;
        StatusFilterText.Foreground = brush;
    }

    /// <summary>
    /// Returns the accent/prominent color for a given status.
    /// For filled badges (Processing, Pending) → uses BgBrush (amber, red-orange).
    /// For outlined badges (Shipped, Delivered, Cancelled) → uses FgBrush (dark, green, grey).
    /// This matches the OrderList page status column color palette.
    /// </summary>
    public static SolidColorBrush GetStatusAccentBrush(string status)
    {
        // For filled statuses (Fg=White in OrderList), use BgBrush as the accent
        // For outlined statuses, use FgBrush as the accent
        var key = status switch
        {
            "Processing" => "StatusProcessingBgBrush",   // #F3B55C amber
            "Shipped" => "StatusShippedFgBrush",          // #2C2118 dark
            "Delivered" => "StatusDeliveredFgBrush",      // #009966 green
            "Pending" => "StatusPendingBgBrush",          // #E07A5F red-orange
            "Cancelled" => "StatusCancelledFgBrush",      // #99A1AF grey
            _ => "StatusShippedFgBrush"
        };

        if (Application.Current.Resources.TryGetValue(key, out var resource) && resource is SolidColorBrush brush)
        {
            return brush;
        }
        return new SolidColorBrush(Colors.Gray);
    }

    public static SolidColorBrush GetStatusBgBrush(string status)
    {
        var key = status switch
        {
            "Processing" => "StatusProcessingBgBrush",
            "Shipped" => "StatusShippedBgBrush",
            "Delivered" => "StatusDeliveredBgBrush",
            "Pending" => "StatusPendingBgBrush",
            "Cancelled" => "StatusCancelledBgBrush",
            _ => "StatusShippedBgBrush"
        };

        if (Application.Current.Resources.TryGetValue(key, out var resource) && resource is SolidColorBrush brush)
        {
            return brush;
        }
        return new SolidColorBrush(Colors.Gray);
    }

    // ─── Header buttons ───
    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        this.Hide();
    }

    private async void SubmitButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is CreateOrderViewModel vm)
        {
            var saved = await vm.SaveAsync();
            if (saved)
            {
                this.Hide();
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

    // ─── Status Flyout ───
    private void StatusFilterList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ListView lv && lv.SelectedItem is string selected && DataContext is CreateOrderViewModel vm)
        {
            vm.SelectedStatus = selected;
            StatusFilterText.Text = selected;
            ApplyStatusColor(selected);
            StatusFilterButton.Flyout?.Hide();
        }
    }

    // ─── Color flyout list items when container is loaded ───
    private void StatusFilterList_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
    {
        if (args.Item is string status && args.ItemContainer?.ContentTemplateRoot is StackPanel panel)
        {
            var accentBrush = GetStatusAccentBrush(status);
            foreach (var child in panel.Children)
            {
                if (child is Ellipse dot)
                {
                    dot.Fill = accentBrush;
                }
                else if (child is TextBlock tb)
                {
                    tb.Foreground = accentBrush;
                }
            }
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
        var nameBox = new TextBox { PlaceholderText = "Product name", Header = "Name" };
        var qtyBox = new NumberBox
        {
            PlaceholderText = "1",
            Header = "Quantity",
            Minimum = 1,
            Value = 1,
            SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Inline
        };
        var priceBox = new NumberBox
        {
            PlaceholderText = "0.00",
            Header = "Unit Price",
            Minimum = 0,
            Value = 0
        };

        panel.Children.Add(nameBox);
        panel.Children.Add(qtyBox);
        panel.Children.Add(priceBox);
        addDialog.Content = panel;

        var result = await addDialog.ShowAsync();
        if (result == ContentDialogResult.Primary && !string.IsNullOrWhiteSpace(nameBox.Text))
        {
            vm.Products.Add(new OrderProductItem
            {
                ProductName = nameBox.Text,
                Quantity = (int)qtyBox.Value,
                UnitPrice = (decimal)priceBox.Value
            });
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
