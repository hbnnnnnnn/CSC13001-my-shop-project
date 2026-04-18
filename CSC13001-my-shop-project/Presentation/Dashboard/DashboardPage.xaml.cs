using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;

namespace CSC13001_my_shop_project.Presentation.Dashboard;

public sealed partial class DashboardPage : Page
{
    public DashboardPage()
    {
        this.InitializeComponent();
        Loaded += (_, _) => WeakReferenceMessenger.Default.Send(new ChromeVisibilityMessage(true));
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

    // ── Interactive chart ─────────────────────────────────────────────────
    //  Canvas size: 880×270  |  Plot area: x=52..876, y=12..220
    //  Y mapping: $0=220, $2k=168, $4k=116, $6k=64, $8k=12
    //  X mapping: day D → x = (D-1)*27.6 + 52

    private static readonly (double X, double Y, string Date, string Value)[] ChartPoints =
    [
        (52, 158, "Mar 1", "$2,385"),
        (79, 130, "Mar 2", "$3,460"),
        (106, 108, "Mar 3", "$4,310"),
        (133, 80, "Mar 4", "$5,385"),
        (160, 46, "Mar 5", "$6,690"),
        (187, 64, "Mar 6", "$6,000"),
        (214, 96, "Mar 7", "$4,770"),
        (241, 116, "Mar 8", "$4,000"),
        (268, 136, "Mar 9", "$3,230"),
        (295, 86, "Mar 10", "$5,150"),
        (322, 56, "Mar 11", "$6,310"),
        (349, 86, "Mar 12", "$5,150"),
        (376, 124, "Mar 13", "$3,690"),
        (403, 104, "Mar 14", "$4,460"),
        (430, 124, "Mar 15", "$3,690"),
        (457, 148, "Mar 16", "$2,770"),
        (484, 112, "Mar 17", "$4,150"),
        (511, 78, "Mar 18", "$5,460"),
        (538, 96, "Mar 19", "$4,770"),
        (565, 68, "Mar 20", "$5,850"),
        (592, 44, "Mar 21", "$7,480"),
        (619, 72, "Mar 22", "$5,690"),
        (646, 108, "Mar 23", "$4,310"),
        (673, 88, "Mar 24", "$5,080"),
        (700, 116, "Mar 25", "$4,000"),
        (727, 104, "Mar 26", "$4,460"),
        (754, 128, "Mar 27", "$3,540"),
        (781, 148, "Mar 28", "$2,770"),
        (808, 136, "Mar 29", "$3,230"),
        (835, 158, "Mar 30", "$2,385"),
        (862, 140, "Mar 31", "$3,080"),
    ];

    private void Chart_PointerMoved(object sender, PointerRoutedEventArgs e)
    {
        var pos = e.GetCurrentPoint(ChartCanvas).Position;

        // Only respond within a generous hit area around the plot
        if (pos.X < 40 || pos.X > 888)
            return;

        // Find nearest data point by X distance
        var nearest = ChartPoints[0];
        var minDist = double.MaxValue;
        foreach (var pt in ChartPoints)
        {
            var dist = Math.Abs(pt.X - pos.X);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = pt;
            }
        }

        // Update tooltip text (two lines: date + value)
        ChartTooltipText.Text = $"{nearest.Date}\n{nearest.Value}";

        // Keep tooltip centered above the point, clamped inside Canvas
        const double ttW = 72,
            ttH = 40;
        var ttLeft = Math.Clamp(nearest.X - ttW / 2.0, 52, 876 - ttW);
        var ttTop = Math.Max(nearest.Y - ttH - 12, 0);
        Canvas.SetLeft(ChartTooltip, ttLeft);
        Canvas.SetTop(ChartTooltip, ttTop);

        // Position outer ring (14×14) and inner dot (8×8) centered on data point
        Canvas.SetLeft(ChartHoverDotOuter, nearest.X - 7);
        Canvas.SetTop(ChartHoverDotOuter, nearest.Y - 7);
        Canvas.SetLeft(ChartHoverDotInner, nearest.X - 4);
        Canvas.SetTop(ChartHoverDotInner, nearest.Y - 4);

        ChartTooltip.Visibility = Visibility.Visible;
        ChartHoverDotOuter.Visibility = Visibility.Visible;
        ChartHoverDotInner.Visibility = Visibility.Visible;
    }

    private void Chart_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        ChartTooltip.Visibility = Visibility.Collapsed;
        ChartHoverDotOuter.Visibility = Visibility.Collapsed;
        ChartHoverDotInner.Visibility = Visibility.Collapsed;
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
            b.Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent);
    }
}
