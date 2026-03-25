using CSC13001_my_shop_project.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Uno.Extensions.Navigation;
using Uno.Extensions.Navigation.UI;

namespace CSC13001_my_shop_project.Presentation.Products;

public sealed partial class ProductsPage : Page
{
    private const int ProductGridColumns = 4;
    private const double ProductGridHorizontalGap = 12;

    public static readonly DependencyProperty ProductGridTileWidthProperty = DependencyProperty.Register(
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
        Loaded += (_, _) => UpdateProductGridTileWidth(ProductsGridView.ActualWidth);
        ProductsGridView.ContainerContentChanging += ProductsGridView_ContainerContentChanging;
    }

    private ProductsViewModel? VM => DataContext as ProductsViewModel;

    private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        if (args.NewValue is ProductsViewModel vm)
            BuildMenuFlyouts(vm);
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
        ToolbarSearchFocusRing.BorderBrush = Application.Current.Resources["ShellAccentBrush"] as Brush;

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
        await nav.NavigateRouteAsync(this, "ProductDetail", data: new ProductDetailArgs(item.Id));
    }

    private async void ProductListRow_Tapped(object sender, TappedRoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.DataContext is not ProductListItem item)
            return;
        var nav = this.Navigator();
        if (nav is null)
            return;
        await nav.NavigateRouteAsync(this, "ProductDetail", data: new ProductDetailArgs(item.Id));
    }
}
