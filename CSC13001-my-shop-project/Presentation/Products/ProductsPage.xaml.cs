using CommunityToolkit.Mvvm.Messaging;
using CSC13001_my_shop_project.Models;
using CSC13001_my_shop_project.Presentation.Dashboard;
using CSC13001_my_shop_project.Services;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Uno.Extensions.Navigation;
using Uno.Extensions.Navigation.UI;

namespace CSC13001_my_shop_project.Presentation.Products;

public sealed partial class ProductsPage : Page
{
    private const int ProductGridColumns = 4;
    private const double ProductGridHorizontalGap = 12;
    private const string StateKey = "ProductsPage";

    private NavigationStateStore? _stateStore;
    private double _pendingScrollOffset = -1;

    public static readonly DependencyProperty ProductGridTileWidthProperty =
        DependencyProperty.Register(
            nameof(ProductGridTileWidth),
            typeof(double),
            typeof(ProductsPage),
            new PropertyMetadata(232.0)
        );

    public double ProductGridTileWidth
    {
        get => (double)GetValue(ProductGridTileWidthProperty);
        set => SetValue(ProductGridTileWidthProperty, value);
    }

    public ProductsPage()
    {
        this.InitializeComponent();
        DataContextChanged += OnDataContextChanged;
        Loaded += OnLoaded;
        ProductsGridView.ContainerContentChanging += ProductsGridView_ContainerContentChanging;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        WeakReferenceMessenger.Default.Send(new ChromeVisibilityMessage(true));
        UpdateProductGridTileWidth(ProductsGridView.ActualWidth);
        ApplyPendingScrollOffset();
        if (VM is not null)
            ArmCreateDialogAfterLayout(VM);
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (VM is not null)
            ArmCreateDialogAfterLayout(VM);
    }

    /// <summary>
    /// Blocks "Add New Product" until after navigation/layout settles so a pointer-up from the sidebar
    /// cannot trigger the command. Also keeps the overlay hidden when bindings are not yet active.
    /// </summary>
    private void ArmCreateDialogAfterLayout(ProductsViewModel vm)
    {
        vm.IsReadyForCreateDialog = false;
        var dq = DispatcherQueue.GetForCurrentThread();
        dq?.TryEnqueue(DispatcherQueuePriority.Low, () =>
        {
            if (DataContext == vm)
                vm.IsReadyForCreateDialog = true;
        });
    }

    private async void ApplyPendingScrollOffset()
    {
        if (_pendingScrollOffset <= 0)
            return;

        var offset = _pendingScrollOffset;
        _pendingScrollOffset = -1;

        // Give the GridView/StackPanel time to realize items and complete layout.
        // Each iteration yields to the UI thread so layout can run between checks.
        for (var attempt = 0; attempt < 10; attempt++)
        {
            await Task.Delay(30);

            if (ProductsScrollViewer.ScrollableHeight >= offset)
            {
                ProductsScrollViewer.ChangeView(null, offset, null, disableAnimation: true);
                return;
            }
        }

        // Best-effort: scroll as far as possible
        if (ProductsScrollViewer.ScrollableHeight > 0)
            ProductsScrollViewer.ChangeView(
                null,
                Math.Min(offset, ProductsScrollViewer.ScrollableHeight),
                null,
                disableAnimation: true
            );
    }

    private ProductsViewModel? VM => DataContext as ProductsViewModel;

    private NavigationStateStore StateStore
    {
        get
        {
            _stateStore ??= (
                App.AppHost?.Services.GetService(typeof(NavigationStateStore))
                as NavigationStateStore
            )!;
            return _stateStore;
        }
    }

    private void SaveCurrentState()
    {
        if (VM is null)
            return;

        StateStore.Save(
            StateKey,
            new ProductListNavigationState
            {
                ScrollOffsetY = ProductsScrollViewer.VerticalOffset,
                SearchQuery = VM.SearchQuery,
                SelectedCategory = VM.SelectedCategory,
                SelectedStatusFilter = VM.SelectedStatusFilter,
                SelectedSort = VM.SelectedSort,
                CurrentPage = VM.CurrentPage,
                IsGridView = VM.IsGridView,
            }
        );
    }

    private void RestoreState(ProductsViewModel vm)
    {
        var state = StateStore.Restore<ProductListNavigationState>(StateKey);
        if (state is null)
            return;

        vm.BulkRestoreState(
            state.SearchQuery,
            state.SelectedCategory,
            state.SelectedStatusFilter,
            state.SelectedSort,
            state.CurrentPage,
            state.IsGridView
        );

        if (state.ScrollOffsetY > 0)
        {
            _pendingScrollOffset = state.ScrollOffsetY;
            ApplyPendingScrollOffset();
        }

        StateStore.Clear(StateKey);
    }

    private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        if (args.NewValue is ProductsViewModel vm)
        {
            RestoreState(vm);
            BuildMenuFlyouts(vm);
            ArmCreateDialogAfterLayout(vm);
        }
    }

    private void BuildMenuFlyouts(ProductsViewModel vm)
    {
        CategoryMenuFlyout.Items.Clear();
        foreach (var opt in vm.CategoryOptions)
        {
            var item = new MenuFlyoutItem { Text = opt };
            item.Click += (_, _) => vm.SelectedCategory = opt;
            CategoryMenuFlyout.Items.Add(item);
        }

        StatusMenuFlyout.Items.Clear();
        foreach (var opt in vm.StatusFilterOptions)
        {
            var item = new MenuFlyoutItem { Text = opt };
            item.Click += (_, _) => vm.SelectedStatusFilter = opt;
            StatusMenuFlyout.Items.Add(item);
        }

        SortMenuFlyout.Items.Clear();
        foreach (var opt in vm.SortOptions)
        {
            var item = new MenuFlyoutItem { Text = opt };
            item.Click += (_, _) => vm.SelectedSort = opt;
            SortMenuFlyout.Items.Add(item);
        }
    }

    private void ProductsGridView_SizeChanged(object sender, SizeChangedEventArgs e) =>
        UpdateProductGridTileWidth(e.NewSize.Width);

    private void UpdateProductGridTileWidth(double gridWidth)
    {
        if (gridWidth <= 1)
            return;
        var gapsBetweenColumns = (ProductGridColumns - 1) * ProductGridHorizontalGap;
        var tile = (gridWidth - gapsBetweenColumns) / ProductGridColumns;
        if (tile < 120)
            tile = 120;
        if (System.Math.Abs(tile - ProductGridTileWidth) > 0.5)
            ProductGridTileWidth = tile;
    }

    private void ProductsGridView_ContainerContentChanging(
        ListViewBase sender,
        ContainerContentChangingEventArgs args
    )
    {
        if (args.ItemContainer is not GridViewItem item)
            return;

        if (args.InRecycleQueue)
        {
            item.Margin = new Thickness(0);
            return;
        }

        if (args.ItemIndex < 0)
            return;

        var isLastInRow = (args.ItemIndex % ProductGridColumns) == ProductGridColumns - 1;
        item.Margin = isLastInRow
            ? new Thickness(0, 0, 0, 16)
            : new Thickness(0, 0, ProductGridHorizontalGap, 16);
    }

    private void SearchBox_GotFocus(object sender, RoutedEventArgs e) =>
        ToolbarSearchFocusRing.BorderBrush =
            Application.Current.Resources["ShellAccentBrush"] as Brush;

    private void SearchBox_LostFocus(object sender, RoutedEventArgs e) =>
        ToolbarSearchFocusRing.BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Transparent);

    private void ProductRow_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (sender is not Border b)
            return;

        b.Background = (Brush)Resources["ProductCardHoverBackgroundBrush"];
        b.BorderBrush = (Brush)Resources["ProductCardHoverBorderBrush"];
        b.BorderThickness = new Thickness(1);
        b.RenderTransform = new TranslateTransform { Y = -2 };
    }

    private void ProductRow_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        if (sender is not Border b)
            return;

        b.Background = (Brush)Resources["ProductCardSurfaceBrush"];
        b.BorderBrush = (Brush)Application.Current.Resources["ShellBorderBrush"];
        b.BorderThickness = new Thickness(1);
        b.RenderTransform = null;
    }

    private async void ProductsGridView_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is not ProductListItem item)
            return;
        var nav = this.Navigator();
        if (nav is null)
            return;
        SaveCurrentState();
        await nav.NavigateRouteAsync(this, "ProductDetail", data: new ProductDetailArgs(item.Id));
    }

    private async void ProductListRow_Tapped(object sender, TappedRoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.DataContext is not ProductListItem item)
            return;
        var nav = this.Navigator();
        if (nav is null)
            return;
        SaveCurrentState();
        await nav.NavigateRouteAsync(this, "ProductDetail", data: new ProductDetailArgs(item.Id));
    }

    private void CreateBackdrop_Tapped(object sender, TappedRoutedEventArgs e)
    {
        if (VM?.CreateDialogViewModel is { } dlg)
            dlg.CancelCommand.Execute(null);
    }
}
