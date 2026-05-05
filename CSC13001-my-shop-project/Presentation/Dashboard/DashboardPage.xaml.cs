using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Shapes;

namespace CSC13001_my_shop_project.Presentation.Dashboard;

public sealed partial class DashboardPage : Page
{
    // ── Chart coordinate system ────────────────────────────────────────
    //  Canvas size: 880×270  |  Plot area: x=52..876, y=12..220
    private const double CanvasW = 880, CanvasH = 270;
    private const double PlotLeft = 52, PlotRight = 876;
    private const double PlotTop = 12, PlotBottom = 220;
    private const double PlotWidth = PlotRight - PlotLeft;   // 824
    private const double PlotHeight = PlotBottom - PlotTop;  // 208

    /// <summary>Computed chart points used for hover interaction.</summary>
    private (double X, double Y, string Date, string Value)[] _chartPoints = [];

    public DashboardPage()
    {
        this.InitializeComponent();
        Loaded += OnPageLoaded;
    }

    private bool _chartEventSubscribed;

    private void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        WeakReferenceMessenger.Default.Send(new ChromeVisibilityMessage(true));

        // Subscribe to chart data ready event (once only)
        if (!_chartEventSubscribed && DataContext is DashboardViewModel vm)
        {
            vm.ChartDataReady += () => DispatcherQueue.TryEnqueue(RedrawChart);
            _chartEventSubscribed = true;
        }

        // Reload all data every time user navigates to Dashboard
        if (DataContext is DashboardViewModel viewModel)
        {
            _ = viewModel.LoadAllDashboardDataAsync();
        }
    }

    // ── Dynamic chart rendering ────────────────────────────────────────

    /// <summary>
    /// Clears the hardcoded chart elements and redraws from ViewModel data.
    /// </summary>
    private void RedrawChart()
    {
        if (DataContext is not DashboardViewModel vm) return;
        var points = vm.RevenueChartPoints;
        if (points.Count == 0) return;

        // ── Remove old dynamic elements (keep only named elements + grid lines) ──
        // We'll clear and rebuild the entire canvas content programmatically
        // But first, save references to interactive elements
        var hoverDotOuter = ChartHoverDotOuter;
        var hoverDotInner = ChartHoverDotInner;
        var tooltip = ChartTooltip;
        var tooltipText = ChartTooltipText;

        ChartCanvas.Children.Clear();

        // ── Compute Y range ──
        var maxRevenue = points.Max(p => p.Revenue);
        var minRevenue = points.Min(p => p.Revenue);

        // Nice Y-axis: round up to a "nice" ceiling
        var yMax = maxRevenue == 0 ? 1_000_000L : NiceCeiling(maxRevenue);
        const long yMin = 0;
        var yRange = yMax - yMin;

        // ── Draw horizontal grid lines + Y labels (5 lines) ──
        var gridLineCount = 5;
        for (int i = 0; i < gridLineCount; i++)
        {
            double ratio = (double)i / (gridLineCount - 1);  // 0 → 1
            double y = PlotTop + ratio * PlotHeight;
            long val = yMax - (long)(ratio * yRange);

            // Grid line
            var line = new Line
            {
                X1 = PlotLeft, Y1 = y,
                X2 = PlotRight, Y2 = y,
                Stroke = new SolidColorBrush(Windows.UI.Color.FromArgb(
                    (byte)(i == gridLineCount - 1 ? 0x22 : 0x10), 0, 0, 0)),
                StrokeThickness = 1
            };
            ChartCanvas.Children.Add(line);

            // Y-axis label
            var label = new TextBlock
            {
                Text = FormatShortCurrency(val),
                FontSize = 11,
                Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(0xFF, 0x9C, 0xA3, 0xAF))
            };
            Canvas.SetLeft(label, 4);
            Canvas.SetTop(label, y - 8);
            ChartCanvas.Children.Add(label);
        }

        // ── Compute data point positions ──
        int n = points.Count;
        var computedPoints = new (double X, double Y, string Date, string Value)[n];
        var polyPoints = new PointCollection();
        var areaPoints = new PointCollection();

        for (int i = 0; i < n; i++)
        {
            var pt = points[i];
            double x = n == 1 ? (PlotLeft + PlotRight) / 2 : PlotLeft + (double)i / (n - 1) * PlotWidth;
            double y = yRange == 0 ? (PlotTop + PlotBottom) / 2 : PlotBottom - (double)(pt.Revenue - yMin) / yRange * PlotHeight;

            computedPoints[i] = (x, y, pt.Label, $"{pt.Revenue:N0} ₫");
            polyPoints.Add(new Windows.Foundation.Point(x, y));

            if (i == 0)
                areaPoints.Add(new Windows.Foundation.Point(x, y));
            else
                areaPoints.Add(new Windows.Foundation.Point(x, y));
        }

        // Close the area polygon at baseline
        if (n > 0)
        {
            areaPoints.Add(new Windows.Foundation.Point(computedPoints[n - 1].X, PlotBottom));
            areaPoints.Add(new Windows.Foundation.Point(computedPoints[0].X, PlotBottom));
        }

        // ── Draw area fill ──
        var areaPolygon = new Polygon
        {
            Points = areaPoints,
            Fill = (Brush)Resources["ChartAreaFillBrush"]
        };
        ChartCanvas.Children.Add(areaPolygon);

        // ── Draw chart line ──
        var chartLine = new Polyline
        {
            Points = polyPoints,
            Stroke = (Brush)Application.Current.Resources["ShellAccentBrush"],
            StrokeThickness = 2.5,
            StrokeLineJoin = PenLineJoin.Round
        };
        ChartCanvas.Children.Add(chartLine);

        // ── Draw X-axis labels (show ~8 evenly spaced) ──
        int maxLabels = Math.Min(8, n);
        for (int i = 0; i < maxLabels; i++)
        {
            int idx = n == 1 ? 0 : (int)Math.Round((double)i / (maxLabels - 1) * (n - 1));
            var label = new TextBlock
            {
                Text = computedPoints[idx].Date,
                FontSize = 11,
                Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(0xFF, 0x9C, 0xA3, 0xAF))
            };
            Canvas.SetLeft(label, computedPoints[idx].X - 16);
            Canvas.SetTop(label, PlotBottom + 8);
            ChartCanvas.Children.Add(label);
        }

        // ── Re-add interactive hover elements ──
        hoverDotOuter.Visibility = Visibility.Collapsed;
        hoverDotInner.Visibility = Visibility.Collapsed;
        tooltip.Visibility = Visibility.Collapsed;

        ChartCanvas.Children.Add(hoverDotOuter);
        ChartCanvas.Children.Add(hoverDotInner);
        ChartCanvas.Children.Add(tooltip);

        // Store computed points for hover interaction
        _chartPoints = computedPoints;
    }

    /// <summary>Rounds up to a "nice" number for Y-axis ceiling.</summary>
    private static long NiceCeiling(long value)
    {
        if (value <= 0) return 1_000_000;
        double mag = Math.Pow(10, Math.Floor(Math.Log10(value)));
        double normalized = value / mag;
        double nice = normalized <= 1 ? 1 : normalized <= 2 ? 2 : normalized <= 5 ? 5 : 10;
        return (long)(nice * mag * 1.2); // Add 20% headroom
    }

    /// <summary>Formats a large number as short currency: 1000000 → "1M ₫", 500000 → "500K ₫"</summary>
    private static string FormatShortCurrency(long value)
    {
        if (value >= 1_000_000_000) return $"{value / 1_000_000_000.0:F1}B ₫";
        if (value >= 1_000_000) return $"{value / 1_000_000.0:F1}M ₫";
        if (value >= 1_000) return $"{value / 1_000.0:F0}K ₫";
        return $"{value:N0} ₫";
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

    // ── Interactive chart hover ───────────────────────────────────────────

    private void Chart_PointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (_chartPoints.Length == 0) return;

        var pos = e.GetCurrentPoint(ChartCanvas).Position;

        // Only respond within a generous hit area around the plot
        if (pos.X < 40 || pos.X > 888)
            return;

        // Find nearest data point by X distance
        var nearest = _chartPoints[0];
        var minDist = double.MaxValue;
        foreach (var pt in _chartPoints)
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
        const double ttW = 90,
            ttH = 40;
        var ttLeft = Math.Clamp(nearest.X - ttW / 2.0, PlotLeft, PlotRight - ttW);
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
            b.Background = new SolidColorBrush(Colors.Transparent);
    }
}

