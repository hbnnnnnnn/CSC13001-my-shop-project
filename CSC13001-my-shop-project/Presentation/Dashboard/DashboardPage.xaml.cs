using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;

namespace CSC13001_my_shop_project.Presentation.Dashboard;

public sealed partial class DashboardPage : Page
{
    public DashboardPage()
    {
        this.InitializeComponent();
        Loaded += OnPageLoaded;
    }

    private async void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        WeakReferenceMessenger.Default.Send(new ChromeVisibilityMessage(true));

        if (DataContext is DashboardViewModel viewModel)
        {
            await viewModel.LoadAllDashboardDataAsync();
        }
    }

    // ── Stat-card float animation ─────────────────────────────────────────

    private void FloatCard(CompositeTransform transform, double toY)
    {
        var anim = new DoubleAnimation
        {
            To = toY,
            Duration = new Duration(TimeSpan.FromMilliseconds(180)),
            EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut },
        };
        var sb = new Storyboard();
        Storyboard.SetTarget(anim, transform);
        Storyboard.SetTargetProperty(anim, "TranslateY");
        sb.Children.Add(anim);
        sb.Begin();
    }

    private void ProductsCard_PointerEntered(object sender, PointerRoutedEventArgs e) =>
        FloatCard(ProductsCardTransform, -5);

    private void ProductsCard_PointerExited(object sender, PointerRoutedEventArgs e) =>
        FloatCard(ProductsCardTransform, 0);

    private void OrdersCard_PointerEntered(object sender, PointerRoutedEventArgs e) =>
        FloatCard(OrdersCardTransform, -5);

    private void OrdersCard_PointerExited(object sender, PointerRoutedEventArgs e) =>
        FloatCard(OrdersCardTransform, 0);

    private void RevenueCard_PointerEntered(object sender, PointerRoutedEventArgs e) =>
        FloatCard(RevenueCardTransform, -5);

    private void RevenueCard_PointerExited(object sender, PointerRoutedEventArgs e) =>
        FloatCard(RevenueCardTransform, 0);

    // ── Navigation shortcuts ──────────────────────────────────────────────

    private void ViewAllProducts_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is DashboardViewModel vm)
            vm.NavigateTo("Products");
    }

    private void ViewAllOrders_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is DashboardViewModel vm)
            vm.NavigateTo("Orders");
    }

    // ── Row hover effect ──────────────────────────────────────────────────

    private void RowBorder_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (sender is Border b)
            b.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(0x08, 0x00, 0x00, 0x00));
    }

    private void RowBorder_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        if (sender is Border b)
            b.Background = new SolidColorBrush(Colors.Transparent);
    }
}

/// <summary>
/// Converts a bool to Visibility: true → Visible, false → Collapsed.
/// WinUI {Binding} does NOT auto-convert bool to Visibility (unlike x:Bind).
/// </summary>
public sealed class DashboardBoolToVisibilityConverter : Microsoft.UI.Xaml.Data.IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) =>
        value is true ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotSupportedException();
}
