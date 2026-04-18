using System.Collections.ObjectModel;
using System.Globalization;
using CSC13001_my_shop_project.Services;
using Uno.Extensions.Navigation;
using Windows.ApplicationModel.DataTransfer;

namespace CSC13001_my_shop_project.Presentation.Products;

public partial class ProductDetailViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IProductService _productService;
    private readonly ProductDetailArgs _args;

    public ProductDetailViewModel(
        INavigator navigator,
        IProductService productService,
        ProductDetailArgs args
    )
    {
        _navigator = navigator;
        _productService = productService;
        _args = args;
        Product = ProductDtoMapping.DetailLoadingModel();
        breadcrumbs =
        [
            new BreadcrumbItem { Label = "All Products", IsClickable = true },
            new BreadcrumbItem { Label = "…", IsClickable = false },
        ];
        _ = LoadProductAsync();
    }

    [ObservableProperty]
    private ProductModel product = null!;

    [ObservableProperty]
    private List<BreadcrumbItem> breadcrumbs;

    public ObservableCollection<OrderModel> RecentOrders { get; } = new();

    private async Task LoadProductAsync()
    {
        try
        {
            var dto = await _productService
                .GetByIdAsync(_args.GraphQlProductId)
                .ConfigureAwait(false);
            App.RunOnUIThread(() =>
            {
                Product = dto is null
                    ? ProductDtoMapping.DetailNotFoundModel(_args.GraphQlProductId)
                    : ProductDtoMapping.ToDetailModel(dto);
                Breadcrumbs =
                [
                    new BreadcrumbItem { Label = "All Products", IsClickable = true },
                    new BreadcrumbItem { Label = Product.Name, IsClickable = false },
                ];
            });
        }
        catch (Exception ex)
        {
            App.RunOnUIThread(() =>
            {
                Product = ProductDtoMapping.DetailErrorModel(ex.Message);
                Breadcrumbs =
                [
                    new BreadcrumbItem { Label = "All Products", IsClickable = true },
                    new BreadcrumbItem { Label = "Error", IsClickable = false },
                ];
            });
        }
    }

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
