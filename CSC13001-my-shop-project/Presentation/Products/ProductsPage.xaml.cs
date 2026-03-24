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
    }

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
