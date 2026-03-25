using System.Collections.ObjectModel;
using System.Globalization;
using Windows.ApplicationModel.DataTransfer;
using Uno.Extensions.Navigation;

namespace CSC13001_my_shop_project.Presentation.Products;

public partial class ProductDetailViewModel : ObservableObject
{
    private readonly INavigator _navigator;

    public ProductDetailViewModel(INavigator navigator, ProductDetailArgs args)
    {
        _navigator = navigator;
        var (p, orders) = ProductDetailFactory.Build(args.ProductId);
        Product = p;
        foreach (var o in orders)
            RecentOrders.Add(o);

        Breadcrumbs =
        [
            new BreadcrumbItem { Label = "All Products", IsClickable = true },
            new BreadcrumbItem { Label = p.Name, IsClickable = false },
        ];
    }

    [ObservableProperty]
    private ProductModel product = null!;

    public ObservableCollection<OrderModel> RecentOrders { get; } = new();

    public List<BreadcrumbItem> Breadcrumbs { get; }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await _navigator.NavigateRouteAsync(this, "Products");
    }

    [RelayCommand]
    private async Task EditProductAsync()
    {
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task DeleteProductAsync()
    {
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task ViewAllOrdersAsync()
    {
        await Task.CompletedTask;
    }

    [RelayCommand]
    private void CopyText(object? value)
    {
        var text = value switch
        {
            null => "",
            string s => s,
            int i => i.ToString(CultureInfo.InvariantCulture),
            long l => l.ToString(CultureInfo.InvariantCulture),
            decimal d => d.ToString(CultureInfo.InvariantCulture),
            _ => value.ToString() ?? "",
        };
        if (string.IsNullOrEmpty(text))
            return;
        var package = new DataPackage();
        package.SetText(text);
        Clipboard.SetContent(package);
    }
}
