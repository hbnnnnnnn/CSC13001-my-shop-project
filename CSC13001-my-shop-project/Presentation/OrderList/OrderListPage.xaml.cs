using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
using System;
using System.Globalization;
using System.Threading.Tasks;

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

    private async void FromDateButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not OrderListViewModel vm)
        {
            return;
        }

        var selectedDate = await PickDateAsync("Select from date", vm.FromDate);
        if (!selectedDate.HasValue)
        {
            return;
        }

        vm.FromDate = selectedDate.Value;
        SetDateLabel(FromDateText, vm.FromDate, FromDatePlaceholder);
        FromDateBorder.BorderBrush = ResolveBrush("ShellAccentBrush", Colors.Orange);
        FromDateBorder.Background = ResolveBrush("ShellNavHoverBackgroundBrush", Colors.Transparent);
    }

    private async void ToDateButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not OrderListViewModel vm)
        {
            return;
        }

        var selectedDate = await PickDateAsync("Select to date", vm.ToDate);
        if (!selectedDate.HasValue)
        {
            return;
        }

        vm.ToDate = selectedDate.Value;
        SetDateLabel(ToDateText, vm.ToDate, ToDatePlaceholder);
        ToDateBorder.BorderBrush = ResolveBrush("ShellAccentBrush", Colors.Orange);
        ToDateBorder.Background = ResolveBrush("ShellNavHoverBackgroundBrush", Colors.Transparent);
    }

    private async Task<DateTimeOffset?> PickDateAsync(string title, DateTimeOffset? currentDate)
    {
        var picker = new DatePicker
        {
            Date = currentDate ?? DateTimeOffset.Now,
            DayVisible = true,
            MonthVisible = true,
            YearVisible = true
        };

        var dialog = new ContentDialog
        {
            XamlRoot = this.XamlRoot,
            Title = title,
            PrimaryButtonText = "Apply",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            Content = picker
        };

        var result = await dialog.ShowAsync();
        return result == ContentDialogResult.Primary ? picker.Date : null;
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
}
