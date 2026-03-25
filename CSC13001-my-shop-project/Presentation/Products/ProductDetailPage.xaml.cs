using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Uno.Extensions.Navigation;
using Uno.Extensions.Navigation.UI;

namespace CSC13001_my_shop_project.Presentation.Products;

public sealed partial class ProductDetailPage : Page
{
    public ProductDetailPage()
    {
        this.InitializeComponent();
    }

    private async void BackButton_Click(object sender, RoutedEventArgs e)
    {
        await NavigateToProductsAsync();
    }

    private async void BreadcrumbBack_Click(object sender, RoutedEventArgs e)
    {
        await NavigateToProductsAsync();
    }

    private async Task NavigateToProductsAsync()
    {
        var nav = this.Navigator();
        if (nav is not null)
            await nav.NavigateRouteAsync(this, "Products");
    }
}
