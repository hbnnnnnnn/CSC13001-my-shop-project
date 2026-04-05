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

    private async void NewOrderButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new CreateOrderDialog
        {
            XamlRoot = this.XamlRoot,
            DataContext = new CreateOrderViewModel()
        };

        await dialog.ShowAsync();
    }

    private async void ViewOrder_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem item && item.DataContext is OrderItem order)
        {
            var dialog = new OrderDetailDialog
            {
                XamlRoot = this.XamlRoot,
                DataContext = new OrderDetailViewModel(order)
            };

            await dialog.ShowAsync();
        }
    }

    private async void EditOrder_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem item && item.DataContext is OrderItem order)
        {
            var dialog = new CreateOrderDialog
            {
                XamlRoot = this.XamlRoot,
                DataContext = new CreateOrderViewModel(order)
            };

            await dialog.ShowAsync();
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
                // Delete the order from ViewModel
                if (DataContext is OrderListViewModel vm)
                {
                    vm.DeleteOrder(order);
                }

                // Show success dialog
                var successDialog = new DeleteSuccessDialog
                {
                    XamlRoot = this.XamlRoot
                };
                successDialog.SetOrderId(order.Id);

                await successDialog.ShowAsync();
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
