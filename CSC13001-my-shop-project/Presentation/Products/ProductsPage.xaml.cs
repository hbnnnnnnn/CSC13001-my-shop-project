using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;

namespace CSC13001_my_shop_project.Presentation.Products;

public sealed partial class ProductsPage : Page
{
    public ProductsPage()
    {
        this.InitializeComponent();
        DataContextChanged += OnDataContextChanged;
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

    private void SearchBox_GotFocus(object sender, RoutedEventArgs e) =>
        ToolbarSearchFocusRing.BorderBrush = Application.Current.Resources["ShellAccentBrush"] as Brush;

    private void SearchBox_LostFocus(object sender, RoutedEventArgs e) =>
        ToolbarSearchFocusRing.BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Transparent);

    private void ProductRow_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (sender is Border b)
            b.Background = Application.Current.Resources["ShellNavHoverBackgroundBrush"] as Brush;
    }

    private void ProductRow_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        if (sender is Border b)
            b.Background = null;
    }
}
