using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows.System;

namespace CSC13001_my_shop_project.Presentation.Products;

public sealed partial class CreateProductDialog : UserControl
{
    private double _layoutBudgetHeight;

    public CreateProductDialog()
    {
        this.InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        InnerGrid.SizeChanged += InnerGrid_SizeChanged;
        LayoutUpdated += OnLayoutUpdated;
    }

    /// <summary>
    /// Sizes the shell to the host. Scroll viewport height uses <see cref="_layoutBudgetHeight"/> so it
    /// matches the explicit <see cref="FrameworkElement.Height"/> even before layout finishes measuring.
    /// </summary>
    public void ApplyLayoutBudget(double hostMaxWidth, double hostMaxHeight)
    {
        var w = Math.Max(320, hostMaxWidth);
        // Do not inflate past what the host passed — short windows need a shorter dialog.
        var h = Math.Max(240, hostMaxHeight);

        _layoutBudgetHeight = h;
        MaxWidth = w;
        MaxHeight = h;
        // Let the parent overlay constrain height (Stretch). A fixed Height fights the grid and
        // breaks layout on some targets (content "escapes" the scroll viewport).
        Height = double.NaN;
        MinHeight = 0;
        // Never force a min wider than the budget (680 caused horizontal overflow + h-scrollbar).
        MinWidth = Math.Min(560, w);

        ShellBorder.MinWidth = Math.Min(560, w);
        // Allow wide hosts (full main-content width); only cap by the passed budget.
        ShellBorder.MaxWidth = w;
        ShellBorder.MaxHeight = h;
    }

    private void OnLoaded(object sender, RoutedEventArgs e) => SyncBodyScrollHeight();

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        LayoutUpdated -= OnLayoutUpdated;
    }

    private void InnerGrid_SizeChanged(object sender, SizeChangedEventArgs e) => SyncBodyScrollHeight();

    private void OnLayoutUpdated(object? sender, object e) => SyncBodyScrollHeight();

    /// <summary>
    /// Skia Desktop: give <see cref="BodyScroll"/> an explicit viewport height from the layout budget
    /// minus the header so vertical scroll works and the last lines are not clipped.
    /// </summary>
    private void SyncBodyScrollHeight()
    {
        void Apply()
        {
            var headerH = HeaderRow.ActualHeight;
            if (headerH < 1)
                return;

            double body;
            var innerH = InnerGrid.ActualHeight;
            if (innerH > headerH + 80)
                body = innerH - headerH - 4;
            else if (_layoutBudgetHeight > headerH + 80)
                body = _layoutBudgetHeight - headerH - 4;
            else
            {
                BodyScroll.ClearValue(FrameworkElement.HeightProperty);
                return;
            }

            BodyScroll.Height = body;
        }

        Apply();
        Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread()?.TryEnqueue(
            Microsoft.UI.Dispatching.DispatcherQueuePriority.Low,
            Apply
        );
    }

    private void TagEntryBox_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key != VirtualKey.Enter || DataContext is not CreateProductViewModel vm)
            return;
        vm.AddTagCommand.Execute(null);
        e.Handled = true;
    }

    private void UploadZone_Tapped(object sender, TappedRoutedEventArgs e)
    {
        if (DataContext is CreateProductViewModel vm)
            vm.AddImagesFromPickerCommand.Execute(null);
    }
}
