using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.Kernel;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using CSC13001_my_shop_project.Models;
using CSC13001_my_shop_project.Services;

namespace CSC13001_my_shop_project.Presentation.Reports;

public partial class ReportViewModel : ObservableObject
{
    private const int TopProductLineCount = 8;
    private const int TopSellingPieLimit = 12;

    private static readonly CultureInfo UsCulture = CultureInfo.GetCultureInfo("en-US");

    private const float AxisLabelTextSize = 10f;
    private const float AxisNameTextSize = 11f;

    private readonly IReportService _reportService;
    private IReadOnlyList<CategorySalesPeriodDto> _cachedCategoryPeriods = [];

    public ReportViewModel(IReportService reportService)
    {
        _reportService = reportService;
        StartDate = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        EndDate = new DateTimeOffset(2026, 12, 31, 0, 0, 0, TimeSpan.Zero);
    }

    private static readonly SKColor[] Palette =
    [
        SKColor.Parse("F3B55C"),
        SKColor.Parse("3B82F6"),
        SKColor.Parse("10B981"),
        SKColor.Parse("8B5CF6"),
        SKColor.Parse("EC4899"),
        SKColor.Parse("06B6D4"),
        SKColor.Parse("F59E0B"),
        SKColor.Parse("6366F1"),
        SKColor.Parse("84CC16"),
    ];

    [ObservableProperty]
    private string _selectedPeriodKey = "day";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsProductMode))]
    [NotifyPropertyChangedFor(nameof(LineChartTitle))]
    private string _selectedLineModeKey = "category";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StartDateDisplay))]
    private DateTimeOffset _startDate;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EndDateDisplay))]
    private DateTimeOffset _endDate;

    public string StartDateDisplay => StartDate.ToString("d", UsCulture);

    public string EndDateDisplay => EndDate.ToString("d", UsCulture);

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _isDrilldownBusy;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private Axis[] _salesLineXAxes = [new Axis { LabelsRotation = -35, TextSize = AxisLabelTextSize }];

    [ObservableProperty]
    private Axis[] _salesLineYAxes =
    [
        new Axis
        {
            Name = "Unit",
            TextSize = AxisLabelTextSize,
            NameTextSize = AxisNameTextSize,
            Labeler = v => ((double)v).ToString("N0", CultureInfo.InvariantCulture),
        },
    ];

    [ObservableProperty]
    private Axis[] _revenueXAxes = [new Axis { LabelsRotation = -35, TextSize = AxisLabelTextSize }];

    [ObservableProperty]
    private Axis[] _revenueYAxes =
    [
        new Axis
        {
            Name = "Số tiền (₫)",
            TextSize = AxisLabelTextSize,
            NameTextSize = AxisNameTextSize,
            Labeler = v => FormatCompactVnd((double)v),
        },
    ];

    [ObservableProperty]
    private ObservableCollection<ISeries> _salesLineSeries = new();

    [ObservableProperty]
    private ObservableCollection<ISeries> _revenueColumnSeries = new();

    [ObservableProperty]
    private ObservableCollection<ISeries> _pieTopProductsSeries = new();

    [ObservableProperty]
    private ObservableCollection<ISeries> _pieBucketRevenueSeries = new();

    [ObservableProperty]
    private ObservableCollection<ChartLegendItemVm> _salesLineLegendItems = new();

    [ObservableProperty]
    private ObservableCollection<ChartLegendItemVm> _revenueLegendItems = new();

    [ObservableProperty]
    private ObservableCollection<ChartLegendItemVm> _pieTopLegendItems = new();

    [ObservableProperty]
    private ObservableCollection<ChartLegendItemVm> _pieBucketLegendItems = new();

    [ObservableProperty]
    private ObservableCollection<CategoryOption> _categoryOptions = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LineChartTitle))]
    private string? _selectedCategoryId;

    [ObservableProperty]
    private string _overviewTotalOrders = "—";

    [ObservableProperty]
    private string _overviewTotalRevenue = "—";

    [ObservableProperty]
    private string _overviewTotalItemsSold = "—";

    [ObservableProperty]
    private string _overviewAvgOrderValue = "—";

    [ObservableProperty]
    private string _overviewMaxOrderValue = "—";

    [ObservableProperty]
    private string _overviewMinOrderValue = "—";

    public string SelectedCategoryName =>
        CategoryOptions.FirstOrDefault(c => c.Id == SelectedCategoryId)?.Name ?? "";

    public bool IsProductMode => SelectedLineModeKey == "product";

    public string LineChartTitle
    {
        get
        {
            if (SelectedLineModeKey == "category")
                return "Units sold by category";
            return string.IsNullOrWhiteSpace(SelectedCategoryName)
                ? "Units sold by product"
                : $"Units sold: {SelectedCategoryName}";
        }
    }

    public IReadOnlyList<ReportPeriodOption> PeriodOptions { get; } =
    [
        new() { Key = "day", Label = "Daily" },
        new() { Key = "week", Label = "Weekly" },
        new() { Key = "month", Label = "Monthly" },
        new() { Key = "year", Label = "Yearly" },
    ];

    public IReadOnlyList<ReportLineModeOption> LineModeOptions { get; } =
    [
        new() { Key = "category", Label = "Category" },
        new() { Key = "product", Label = "Products" },
    ];

    [RelayCommand]
    private async Task RefreshAsync()
    {
        ErrorMessage = null;
        IsBusy = true;
        try
        {
            var start = StartDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            var end = EndDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            var bundle = await _reportService
                .LoadReportsAsync(SelectedPeriodKey, start, end, TopSellingPieLimit)
                .ConfigureAwait(true);

            _cachedCategoryPeriods = bundle.CategorySales;
            ApplySalesOverview(bundle.SalesOverview);
            RenderLineChart();
            BuildRevenueColumns(bundle.Revenue);
            BuildPieTop(bundle.TopSelling);
            BuildPieBuckets(bundle.Revenue);
        }
        catch (GraphQlException ex)
        {
            ErrorMessage = ex.Message;
            ClearCharts();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            ClearCharts();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void ToggleSeriesVisibility(object? parameter)
    {
        ChartElement? element = parameter switch
        {
            ChartLegendItemVm vm => vm.Series as ChartElement,
            ChartElement ce => ce,
            _ => null,
        };
        if (element is null)
            return;
        element.IsVisible = !element.IsVisible;
        if (parameter is ChartLegendItemVm legendVm)
            legendVm.IsVisibleInChart = element.IsVisible;
    }

    private void ClearCharts()
    {
        _cachedCategoryPeriods = [];
        SalesLineSeries = new ObservableCollection<ISeries>();
        RevenueColumnSeries = new ObservableCollection<ISeries>();
        PieTopProductsSeries = new ObservableCollection<ISeries>();
        PieBucketRevenueSeries = new ObservableCollection<ISeries>();
        SalesLineLegendItems = new ObservableCollection<ChartLegendItemVm>();
        RevenueLegendItems = new ObservableCollection<ChartLegendItemVm>();
        PieTopLegendItems = new ObservableCollection<ChartLegendItemVm>();
        PieBucketLegendItems = new ObservableCollection<ChartLegendItemVm>();
        CategoryOptions = new ObservableCollection<CategoryOption>();
        SelectedCategoryId = null;
        ClearOverviewKpis();
    }

    private void ClearOverviewKpis()
    {
        OverviewTotalOrders = "—";
        OverviewTotalRevenue = "—";
        OverviewTotalItemsSold = "—";
        OverviewAvgOrderValue = "—";
        OverviewMaxOrderValue = "—";
        OverviewMinOrderValue = "—";
    }

    private void ApplySalesOverview(SalesOverviewDto? o)
    {
        if (o is null)
        {
            ClearOverviewKpis();
            return;
        }

        OverviewTotalOrders = o.TotalOrders.ToString("N0", CultureInfo.InvariantCulture);
        OverviewTotalRevenue = FormatCompactVnd(o.TotalRevenue);
        OverviewTotalItemsSold = o.TotalItemsSold.ToString("N0", CultureInfo.InvariantCulture);
        OverviewAvgOrderValue = FormatCompactVnd(o.AvgOrderValue);
        OverviewMaxOrderValue = FormatCompactVnd(o.MaxOrderValue);
        OverviewMinOrderValue = FormatCompactVnd(o.MinOrderValue);
    }

    private static ObservableCollection<ChartLegendItemVm> CreateLegendItems(IEnumerable<ISeries> series) =>
        new(series.Select(s => new ChartLegendItemVm(s)));

    private void BuildCategoryLineChart(IReadOnlyList<CategorySalesPeriodDto> periods)
    {
        var ordered = periods.OrderBy(p => NormalizeSortKey(p.Date)).ToList();
        if (ordered.Count == 0)
        {
            SalesLineSeries = new ObservableCollection<ISeries>();
            SalesLineLegendItems = new ObservableCollection<ChartLegendItemVm>();
            CategoryOptions = new ObservableCollection<CategoryOption>();
            SalesLineXAxes = [new Axis { LabelsRotation = -35, TextSize = AxisLabelTextSize }];
            SalesLineYAxes =
            [
                new Axis
                {
                    Name = "Unit",
                    TextSize = AxisLabelTextSize,
                    NameTextSize = AxisNameTextSize,
                    Labeler = v => ((double)v).ToString("N0", CultureInfo.InvariantCulture),
                },
            ];
            SelectedCategoryId = null;
            OnPropertyChanged(nameof(SalesLineXAxes));
            OnPropertyChanged(nameof(SalesLineYAxes));
            OnPropertyChanged(nameof(SelectedCategoryName));
            OnPropertyChanged(nameof(LineChartTitle));
            return;
        }

        var labels = ordered.Select(p => FormatBucketLabel(SelectedPeriodKey, p.Date, p.Period)).ToList();
        var dateKeys = ordered.Select(p => NormalizeDateKey(p.Date)).ToList();

        var categoryTotals = new Dictionary<string, (string Name, int Qty)>(StringComparer.Ordinal);
        foreach (var row in ordered)
        {
            foreach (var line in row.Categories)
            {
                if (string.IsNullOrWhiteSpace(line.CategoryId))
                    continue;
                if (!categoryTotals.TryGetValue(line.CategoryId, out var t))
                    categoryTotals[line.CategoryId] = (ShortName(line.CategoryName, 20), line.Quantity);
                else
                    categoryTotals[line.CategoryId] = (t.Name, t.Qty + line.Quantity);
            }
        }

        var orderedCategories = categoryTotals
            .OrderByDescending(kv => kv.Value.Qty)
            .Select(kv => (Id: kv.Key, Name: kv.Value.Name))
            .ToList();

        var indexByKey = dateKeys.Select((k, i) => (k, i)).ToDictionary(t => t.k, t => t.i);

        var seriesList = new List<ISeries>();
        var seriesIndex = 0;
        foreach (var cat in orderedCategories)
        {
            var values = new ObservableCollection<double>();
            for (var i = 0; i < dateKeys.Count; i++)
                values.Add(0);

            foreach (var row in ordered)
            {
                var key = NormalizeDateKey(row.Date);
                if (!indexByKey.TryGetValue(key, out var idx))
                    continue;
                var line = row.Categories.FirstOrDefault(c => c.CategoryId == cat.Id);
                if (line is not null)
                    values[idx] += line.Quantity;
            }

            var color = Palette[seriesIndex % Palette.Length];
            seriesIndex++;
            seriesList.Add(
                new LineSeries<double>
                {
                    Name = cat.Name,
                    Values = values,
                    GeometrySize = 6,
                    LineSmoothness = 0.2,
                    Stroke = new SolidColorPaint(color, 2),
                    Fill = null,
                    GeometryStroke = new SolidColorPaint(color, 2),
                    GeometryFill = new SolidColorPaint(color),
                }
            );
        }

        SalesLineSeries = new ObservableCollection<ISeries>(seriesList);
        SalesLineLegendItems = CreateLegendItems(seriesList);
        CategoryOptions = new ObservableCollection<CategoryOption>(
            orderedCategories.Select(c => new CategoryOption { Id = c.Id, Name = c.Name }));

        if (!string.IsNullOrWhiteSpace(SelectedCategoryId) &&
            CategoryOptions.Any(c => c.Id == SelectedCategoryId))
        {
            OnPropertyChanged(nameof(SelectedCategoryName));
            OnPropertyChanged(nameof(LineChartTitle));
        }
        else
        {
            SelectedCategoryId = CategoryOptions.FirstOrDefault()?.Id;
        }

        SalesLineXAxes =
        [
            new Axis
            {
                Labels = labels,
                LabelsRotation = -35,
                TextSize = AxisLabelTextSize,
                SeparatorsPaint = new SolidColorPaint(SKColor.Parse("334155")) { StrokeThickness = 0.5f },
            },
        ];
        SalesLineYAxes =
        [
            new Axis
            {
                Name = "Unit",
                TextSize = AxisLabelTextSize,
                NameTextSize = AxisNameTextSize,
                Labeler = v => ((double)v).ToString("N0", CultureInfo.InvariantCulture),
                SeparatorsPaint = new SolidColorPaint(SKColor.Parse("334155")) { StrokeThickness = 0.5f },
            },
        ];
        OnPropertyChanged(nameof(SalesLineXAxes));
        OnPropertyChanged(nameof(SalesLineYAxes));
    }

    private async Task LoadCategoryBreakdownAsync()
    {
        if (string.IsNullOrWhiteSpace(SelectedCategoryId))
        {
            BuildProductLineChart([]);
            return;
        }

        IsDrilldownBusy = true;
        try
        {
            var start = StartDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            var end = EndDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            var periods = await _reportService
                .LoadProductSalesAsync(SelectedPeriodKey, start, end, SelectedCategoryId)
                .ConfigureAwait(true);
            BuildProductLineChart(periods);
        }
        catch (GraphQlException ex)
        {
            ErrorMessage = ex.Message;
            BuildProductLineChart([]);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            BuildProductLineChart([]);
        }
        finally
        {
            IsDrilldownBusy = false;
        }
    }

    private void BuildProductLineChart(IReadOnlyList<ProductSalesPeriodDto> periods)
    {
        var ordered = periods.OrderBy(p => NormalizeSortKey(p.Date)).ToList();
        if (ordered.Count == 0)
        {
            SalesLineSeries = new ObservableCollection<ISeries>();
            SalesLineLegendItems = new ObservableCollection<ChartLegendItemVm>();
            SalesLineXAxes = [new Axis { LabelsRotation = -35, TextSize = AxisLabelTextSize }];
            SalesLineYAxes =
            [
                new Axis
                {
                    Name = "Unit",
                    TextSize = AxisLabelTextSize,
                    NameTextSize = AxisNameTextSize,
                    Labeler = v => ((double)v).ToString("N0", CultureInfo.InvariantCulture),
                },
            ];
            OnPropertyChanged(nameof(SalesLineXAxes));
            OnPropertyChanged(nameof(SalesLineYAxes));
            return;
        }

        var labels = ordered.Select(p => FormatBucketLabel(SelectedPeriodKey, p.Date, p.Period)).ToList();
        var dateKeys = ordered.Select(p => NormalizeDateKey(p.Date)).ToList();

        var productTotals = new Dictionary<string, (string Name, int Qty)>(StringComparer.Ordinal);
        foreach (var row in ordered)
        {
            foreach (var line in row.Products)
            {
                if (!productTotals.TryGetValue(line.ProductId, out var t))
                    productTotals[line.ProductId] = (ShortName(line.Name, 16), line.Quantity);
                else
                    productTotals[line.ProductId] = (t.Name, t.Qty + line.Quantity);
            }
        }

        var topIds = productTotals
            .OrderByDescending(kv => kv.Value.Qty)
            .Select(kv => kv.Key)
            .Take(TopProductLineCount)
            .ToHashSet(StringComparer.Ordinal);

        var indexByKey = dateKeys.Select((k, i) => (k, i)).ToDictionary(t => t.k, t => t.i);

        var seriesList = new List<ISeries>();
        var seriesIndex = 0;
        foreach (var pid in topIds)
        {
            var name = productTotals[pid].Name;
            var values = new ObservableCollection<double>();
            for (var i = 0; i < dateKeys.Count; i++)
                values.Add(0);

            foreach (var row in ordered)
            {
                var key = NormalizeDateKey(row.Date);
                if (!indexByKey.TryGetValue(key, out var idx))
                    continue;
                var line = row.Products.FirstOrDefault(p => p.ProductId == pid);
                if (line is not null)
                    values[idx] += line.Quantity;
            }

            var color = Palette[seriesIndex % Palette.Length];
            seriesIndex++;
            seriesList.Add(
                new LineSeries<double>
                {
                    Name = name,
                    Values = values,
                    GeometrySize = 6,
                    LineSmoothness = 0.2,
                    Stroke = new SolidColorPaint(color, 2),
                    Fill = null,
                    GeometryStroke = new SolidColorPaint(color, 2),
                    GeometryFill = new SolidColorPaint(color),
                }
            );
        }

        if (productTotals.Count > topIds.Count)
        {
            var values = new ObservableCollection<double>();
            for (var i = 0; i < dateKeys.Count; i++)
                values.Add(0);

            foreach (var row in ordered)
            {
                var key = NormalizeDateKey(row.Date);
                if (!indexByKey.TryGetValue(key, out var idx))
                    continue;
                var other = row.Products.Where(p => !topIds.Contains(p.ProductId)).Sum(p => p.Quantity);
                values[idx] += other;
            }

            var c = SKColor.Parse("9CA3AF");
            seriesList.Add(
                new LineSeries<double>
                {
                    Name = "Other",
                    Values = values,
                    GeometrySize = 5,
                    LineSmoothness = 0.2,
                    Stroke = new SolidColorPaint(c, 2),
                    Fill = null,
                    GeometryStroke = new SolidColorPaint(c, 2),
                    GeometryFill = new SolidColorPaint(c),
                }
            );
        }

        SalesLineSeries = new ObservableCollection<ISeries>(seriesList);
        SalesLineLegendItems = CreateLegendItems(seriesList);

        SalesLineXAxes =
        [
            new Axis
            {
                Labels = labels,
                LabelsRotation = -35,
                TextSize = AxisLabelTextSize,
                SeparatorsPaint = new SolidColorPaint(SKColor.Parse("334155")) { StrokeThickness = 0.5f },
            },
        ];
        SalesLineYAxes =
        [
            new Axis
            {
                Name = "Unit",
                TextSize = AxisLabelTextSize,
                NameTextSize = AxisNameTextSize,
                Labeler = v => ((double)v).ToString("N0", CultureInfo.InvariantCulture),
                SeparatorsPaint = new SolidColorPaint(SKColor.Parse("334155")) { StrokeThickness = 0.5f },
            },
        ];
        OnPropertyChanged(nameof(SalesLineXAxes));
        OnPropertyChanged(nameof(SalesLineYAxes));
    }

    partial void OnSelectedCategoryIdChanged(string? value)
    {
        OnPropertyChanged(nameof(SelectedCategoryName));
        OnPropertyChanged(nameof(LineChartTitle));
        if (IsProductMode)
            _ = LoadCategoryBreakdownAsync();
    }

    partial void OnSelectedLineModeKeyChanged(string? value)
    {
        OnPropertyChanged(nameof(IsProductMode));
        OnPropertyChanged(nameof(LineChartTitle));
        RenderLineChart();
    }

    private void RenderLineChart()
    {
        if (SelectedLineModeKey == "product")
        {
            EnsureCategorySelection();
            _ = LoadCategoryBreakdownAsync();
        }
        else
        {
            BuildCategoryLineChart(_cachedCategoryPeriods);
        }
    }

    private void EnsureCategorySelection()
    {
        if (!string.IsNullOrWhiteSpace(SelectedCategoryId) &&
            CategoryOptions.Any(c => c.Id == SelectedCategoryId))
            return;

        SelectedCategoryId = CategoryOptions.FirstOrDefault()?.Id;
    }

    private void BuildRevenueColumns(IReadOnlyList<RevenuePeriodDto> periods)
    {
        var ordered = periods.OrderBy(p => NormalizeSortKey(p.Date)).ToList();
        if (ordered.Count == 0)
        {
            RevenueColumnSeries = new ObservableCollection<ISeries>();
            RevenueLegendItems = new ObservableCollection<ChartLegendItemVm>();
            RevenueXAxes = [new Axis { LabelsRotation = -35, TextSize = AxisLabelTextSize }];
            RevenueYAxes =
            [
                new Axis
                {
                    Name = "Số tiền (₫)",
                    TextSize = AxisLabelTextSize,
                    NameTextSize = AxisNameTextSize,
                    Labeler = v => FormatCompactVnd((double)v),
                },
            ];
            OnPropertyChanged(nameof(RevenueXAxes));
            OnPropertyChanged(nameof(RevenueYAxes));
            return;
        }

        var labels = ordered.Select(p => FormatBucketLabel(SelectedPeriodKey, p.Date, p.Period)).ToList();
        var rev = new ObservableCollection<double>(ordered.Select(p => (double)p.TotalRevenue));
        var profits = new ObservableCollection<double>(ordered.Select(p => (double)p.TotalProfit));

        RevenueColumnSeries = new ObservableCollection<ISeries>(
        [
            new ColumnSeries<double>
            {
                Name = "Revenue",
                Values = rev,
                ScalesYAt = 0,
                Fill = new SolidColorPaint(SKColor.Parse("F3B55C")),
                Stroke = new SolidColorPaint(SKColor.Parse("D97706"), 1),
                MaxBarWidth = 24,
            },
            new ColumnSeries<double>
            {
                Name = "Profit",
                Values = profits,
                ScalesYAt = 0,
                Fill = new SolidColorPaint(SKColor.Parse("3B82F6")),
                Stroke = new SolidColorPaint(SKColor.Parse("1D4ED8"), 1),
                MaxBarWidth = 24,
            },
        ]);

        var revenueSeriesList = RevenueColumnSeries.ToList();
        RevenueLegendItems = CreateLegendItems(revenueSeriesList);

        RevenueXAxes =
        [
            new Axis
            {
                Labels = labels,
                LabelsRotation = -35,
                TextSize = AxisLabelTextSize,
                SeparatorsPaint = new SolidColorPaint(SKColor.Parse("334155")) { StrokeThickness = 0.5f },
            },
        ];
        RevenueYAxes =
        [
            new Axis
            {
                Name = "Số tiền (₫)",
                TextSize = AxisLabelTextSize,
                NameTextSize = AxisNameTextSize,
                Labeler = v => FormatCompactVnd((double)v),
                SeparatorsPaint = new SolidColorPaint(SKColor.Parse("334155")) { StrokeThickness = 0.5f },
            },
        ];
        OnPropertyChanged(nameof(RevenueXAxes));
        OnPropertyChanged(nameof(RevenueYAxes));
    }

    private void BuildPieTop(IReadOnlyList<TopSellingProductDto> top)
    {
        var list = new List<ISeries>();
        foreach (var p in top.OrderByDescending(x => x.TotalRevenue))
        {
            list.Add(
                new PieSeries<double>
                {
                    Name = ShortName(p.Name, 18),
                    Values = new ObservableCollection<double> { p.TotalRevenue },
                    DataLabelsSize = 10,
                    DataLabelsMaxWidth = 72,
                    DataLabelsPaint = new SolidColorPaint(SKColors.White),
                    DataLabelsFormatter = pt => FormatCompactVnd(pt.Coordinate.PrimaryValue),
                }
            );
        }

        PieTopProductsSeries = new ObservableCollection<ISeries>(list);
        PieTopLegendItems = CreateLegendItems(list);
    }

    private void BuildPieBuckets(IReadOnlyList<RevenuePeriodDto> periods)
    {
        var ordered = periods.OrderBy(p => NormalizeSortKey(p.Date)).ToList();
        var list = new List<ISeries>();
        foreach (var p in ordered)
        {
            var label = FormatBucketLabel(SelectedPeriodKey, p.Date, p.Period);
            list.Add(
                new PieSeries<double>
                {
                    Name = label,
                    Values = new ObservableCollection<double> { p.TotalRevenue },
                    DataLabelsPaint = null,
                }
            );
        }

        PieBucketRevenueSeries = new ObservableCollection<ISeries>(list);
        PieBucketLegendItems = CreateLegendItems(list);
    }

    private static string FormatCompactVnd(double vnd)
    {
        if (double.IsNaN(vnd) || double.IsInfinity(vnd))
            return "0 ₫";
        var sign = vnd < 0 ? "-" : "";
        var x = Math.Abs(vnd);
        if (x >= 1_000_000d)
        {
            var m = x / 1_000_000d;
            return sign + (Math.Abs(m - Math.Round(m)) < 0.0001 ? $"{Math.Round(m):0}M" : $"{m:0.##}M") + " ₫";
        }
        if (x >= 1_000d)
        {
            var k = x / 1_000d;
            return sign + (Math.Abs(k - Math.Round(k)) < 0.0001 ? $"{Math.Round(k):0}K" : $"{k:0.##}K") + " ₫";
        }
        if (x >= 1)
            return sign + Math.Round(vnd, 0).ToString("N0", CultureInfo.InvariantCulture) + " ₫";
        return sign + vnd.ToString("0.##", CultureInfo.InvariantCulture) + " ₫";
    }

    private static string ShortName(string name, int max = 22)
    {
        if (string.IsNullOrEmpty(name))
            return "?";
        return name.Length <= max ? name : name[..(max - 1)] + "…";
    }

    private static string NormalizeDateKey(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return "";
        var s = raw.Trim();
        if (s.Length >= 10 && s[4] == '-' && s[7] == '-')
            return s[..10];
        return s;
    }

    private static long NormalizeSortKey(string raw)
    {
        var key = NormalizeDateKey(raw);
        if (DateTime.TryParse(key, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dt))
            return dt.Ticks;
        return 0;
    }

    private static string FormatBucketLabel(string period, string dateRaw, string periodToken)
    {
        var token = (periodToken ?? "").Trim();
        switch (period)
        {
            case "day":
                if (TryParseBucketDate(dateRaw, out var dDay))
                    return dDay.ToString("M/d/yy", UsCulture);
                if (token.Length >= 10 && DateTime.TryParse(token.AsSpan(0, 10), CultureInfo.InvariantCulture, DateTimeStyles.None, out var dTok))
                    return dTok.ToString("M/d/yy", UsCulture);
                return token.Length >= 10 ? token[..10] : token;
            case "week":
                if (token.Length >= 6 && token[4] == '-')
                {
                    var yy = token[..4][^2..];
                    var w = token[5..].TrimStart('0');
                    if (w.Length == 0)
                        w = "0";
                    return $"W{w}/'{yy}";
                }
                if (TryParseBucketDate(dateRaw, out var dW))
                    return dW.ToString("M/d/yy", UsCulture);
                return token;
            case "month":
                if (token.Length >= 7 && token[4] == '-' &&
                    int.TryParse(token.AsSpan(0, 4), CultureInfo.InvariantCulture, out var y) &&
                    int.TryParse(token.AsSpan(5, 2), CultureInfo.InvariantCulture, out var m))
                    return new DateTime(y, m, 1).ToString("MMM yyyy", UsCulture);
                if (TryParseBucketDate(dateRaw, out var dM))
                    return new DateTime(dM.Year, dM.Month, 1).ToString("MMM yyyy", UsCulture);
                return token;
            case "year":
                return token.Length >= 4 ? token[..4] : token;
            default:
                return string.IsNullOrEmpty(token) ? NormalizeDateKey(dateRaw) : token;
        }
    }

    private static bool TryParseBucketDate(string dateRaw, out DateTime dt)
    {
        dt = default;
        var key = NormalizeDateKey(dateRaw);
        if (key.Length < 10)
            return false;
        return DateTime.TryParse(key.AsSpan(0, 10), CultureInfo.InvariantCulture, DateTimeStyles.None, out dt);
    }

}

public sealed partial class ChartLegendItemVm : ObservableObject
{
    public ISeries Series { get; }

    public string DisplayName => Series.Name ?? "";

    [ObservableProperty]
    private bool _isVisibleInChart;

    public ChartLegendItemVm(ISeries series)
    {
        Series = series;
        _isVisibleInChart = series is not ChartElement ce || ce.IsVisible;
    }
}

public sealed class ReportPeriodOption
{
    public string Key { get; init; } = "";

    public string Label { get; init; } = "";
}

public sealed class ReportLineModeOption
{
    public string Key { get; init; } = "";

    public string Label { get; init; } = "";
}

public sealed class CategoryOption
{
    public string Id { get; init; } = "";

    public string Name { get; init; } = "";
}
