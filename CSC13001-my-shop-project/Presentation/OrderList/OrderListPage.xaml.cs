using CSC13001_my_shop_project.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
using System;
using System.Globalization;

namespace CSC13001_my_shop_project.Presentation.OrderList;

public sealed partial class OrderListPage : Page
{
    private const string DateFormat = "dd/MM/yyyy";
    private const string FromDatePlaceholder = "From Date";
    private const string ToDatePlaceholder = "To Date";

    public OrderListPage()
    {
        this.InitializeComponent();
        this.Loaded += (_, _) =>
        {
            if (DataContext is OrderListViewModel vm)
            {
                SetDateLabel(FromDateText, vm.FromDate, FromDatePlaceholder);
                SetDateLabel(ToDateText, vm.ToDate, ToDatePlaceholder);
            }
        };
    }

    /// <summary>
    /// Resolves a service from the DI container.
    /// </summary>
    private T? GetService<T>() where T : class
        => App.AppHost?.Services.GetService<T>();

    private async void NewOrderButton_Click(object sender, RoutedEventArgs e)
    {
        var orderService = GetService<OrderService>();
        var authService = GetService<AuthService>();
        if (orderService is null || authService is null) return;

        var dialog = new CreateOrderDialog
        {
            XamlRoot = this.XamlRoot,
            DataContext = new CreateOrderViewModel(orderService, authService)
        };

        // Show dialog (fire-and-forget internally)
        _ = dialog.ShowAsync();

        // Wait for the dialog to signal completion
        var success = await dialog.WaitForResultAsync();

        if (success)
        {
            // Show success notification
            var successDialog = new ContentDialog
            {
                XamlRoot = this.XamlRoot,
                Title = "Success",
                Content = "Order has been created successfully!",
                CloseButtonText = "OK"
            };
            await successDialog.ShowAsync();

            // Refresh order list immediately
            if (DataContext is OrderListViewModel vm)
            {
                await vm.LoadOrdersAsync();
            }
        }
    }

    private async void ViewOrder_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem item && item.DataContext is OrderItem order)
        {
            var orderService = GetService<OrderService>();

            // Fetch full order details (with product names) from backend
            OrderItem detailedOrder = order;
            if (orderService is not null)
            {
                try
                {
                    // Extract numeric ID from "#123" format
                    var rawId = order.Id.TrimStart('#');
                    detailedOrder = await orderService.GetOrderByIdAsync(rawId);
                }
                catch
                {
                    // Fallback to the list data if detail fetch fails
                    detailedOrder = order;
                }
            }

            var dialog = new OrderDetailDialog
            {
                XamlRoot = this.XamlRoot,
                DataContext = new OrderDetailViewModel(detailedOrder)
            };

            await dialog.ShowAsync();
        }
    }

    private async void EditOrder_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem item && item.DataContext is OrderItem order)
        {
<<<<<<< Updated upstream
            var orderService = GetService<OrderService>();
            var authService = GetService<AuthService>();
            if (orderService is null || authService is null) return;

            var dialog = new CreateOrderDialog
            {
                XamlRoot = this.XamlRoot,
                DataContext = new CreateOrderViewModel(order, orderService, authService)
=======
<<<<<<< Updated upstream
            var dialog = new CreateOrderDialog
            {
                XamlRoot = this.XamlRoot,
                DataContext = new CreateOrderViewModel(order)
=======
            var orderService = GetService<OrderService>();
            var authService = GetService<AuthService>();
            if (orderService is null || authService is null) return;

            // Fetch full details for edit
            OrderItem detailedOrder = order;
            try
            {
                var rawId = order.Id.TrimStart('#');
                detailedOrder = await orderService.GetOrderByIdAsync(rawId);
            }
            catch
            {
                detailedOrder = order;
            }

            var dialog = new CreateOrderDialog
            {
                XamlRoot = this.XamlRoot,
                DataContext = new CreateOrderViewModel(detailedOrder, orderService, authService)
>>>>>>> Stashed changes
>>>>>>> Stashed changes
            };

            // Show dialog and wait for result
            _ = dialog.ShowAsync();
            var success = await dialog.WaitForResultAsync();

            if (success)
            {
                var successDialog = new ContentDialog
                {
                    XamlRoot = this.XamlRoot,
                    Title = "Success",
                    Content = "Order has been updated successfully!",
                    CloseButtonText = "OK"
                };
                await successDialog.ShowAsync();

                // Refresh order list
                if (DataContext is OrderListViewModel vm)
                {
                    await vm.LoadOrdersAsync();
                }
            }
        }
    }

    private async void DeleteOrder_Click(object sender, RoutedEventArgs e)
    {
        OrderItem? order = null;

        // Try to get OrderItem from MenuFlyoutItem DataContext
        if (sender is MenuFlyoutItem item)
        {
            order = item.DataContext as OrderItem;
        }

        if (order == null) return;

        try
        {
            // Show delete confirmation dialog
            var confirmDialog = new DeleteOrderDialog
            {
                XamlRoot = this.XamlRoot
            };
            confirmDialog.SetOrderId(order.Id);

            await confirmDialog.ShowAsync();

            if (confirmDialog.IsConfirmed)
            {
                if (DataContext is OrderListViewModel vm)
                {
                    // Call backend soft-delete
                    var deleted = await vm.DeleteOrderAsync(order);

                    if (deleted)
                    {
                        // Show success dialog
                        var successDialog = new DeleteSuccessDialog
                        {
                            XamlRoot = this.XamlRoot
                        };
                        successDialog.SetOrderId(order.Id);
                        await successDialog.ShowAsync();
                    }
                    else if (!string.IsNullOrEmpty(vm.ErrorMessage))
                    {
                        // Show error dialog (e.g., cannot delete shipped/delivered order)
                        var errorDialog = new ContentDialog
                        {
                            XamlRoot = this.XamlRoot,
                            Title = "Cannot Delete Order",
                            Content = vm.ErrorMessage,
                            CloseButtonText = "OK"
                        };
                        await errorDialog.ShowAsync();
                        vm.ErrorMessage = null;
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"DeleteOrder error: {ex.Message}");
        }
    }

    private void RowBorder_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (sender is Border border)
        {
            border.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(0x08, 0x00, 0x00, 0x00));
        }
    }

    private void RowBorder_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        if (sender is Border border)
        {
            border.Background = new SolidColorBrush(Colors.Transparent);
        }
    }

    private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
    {
        SearchBoxBorder.BorderBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(0xFF, 0xF3, 0xB5, 0x5C));
    }

    private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
    {
        SearchBoxBorder.BorderBrush = new SolidColorBrush(Colors.Transparent);
    }

    // Đóng flyout và cập nhật text khi chọn status
    private void StatusFilterList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (StatusFilterButton.Flyout is Flyout flyout)
        {
            flyout.Hide();
        }
    }

    private void FromDateFlyout_DatePicked(DatePickerFlyout sender, DatePickedEventArgs args)
    {
        if (DataContext is not OrderListViewModel vm)
        {
            return;
        }

        vm.FromDate = args.NewDate;
        SetDateLabel(FromDateText, vm.FromDate, FromDatePlaceholder);
    }

    private void ToDateFlyout_DatePicked(DatePickerFlyout sender, DatePickedEventArgs args)
    {
        if (DataContext is not OrderListViewModel vm)
        {
            return;
        }

        vm.ToDate = args.NewDate;
        SetDateLabel(ToDateText, vm.ToDate, ToDatePlaceholder);
    }

    private void SetDateLabel(TextBlock textBlock, DateTimeOffset? value, string placeholder)
    {
        if (!value.HasValue)
        {
            textBlock.Text = placeholder;
            textBlock.Foreground = ResolveBrush("ShellTextMutedBrush", Colors.Gray);
            return;
        }

        textBlock.Text = value.Value.ToString(DateFormat, CultureInfo.InvariantCulture);
        textBlock.Foreground = ResolveBrush("ShellTextStrongBrush", Colors.Black);
    }

    private SolidColorBrush ResolveBrush(string resourceKey, Windows.UI.Color fallbackColor)
    {
        if (Application.Current.Resources.TryGetValue(resourceKey, out var resource) && resource is SolidColorBrush brush)
        {
            return brush;
        }

        return new SolidColorBrush(fallbackColor);
    }

    // ─── Page Size selector ───
    private void PageSizeList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ListView lv && lv.SelectedItem is int size && DataContext is OrderListViewModel vm)
        {
            vm.PageSize = size;
            PageSizeButton.Flyout?.Hide();
        }
    }

    // ─── Page Number selector ───
    private void PageSelectorList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ListView lv && lv.SelectedItem is int page && DataContext is OrderListViewModel vm)
        {
            vm.CurrentPage = page;
            PageSelectorButton.Flyout?.Hide();
        }
    }
}
