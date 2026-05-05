using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using CSC13001_my_shop_project.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Uno.Extensions.Navigation;
using Windows.ApplicationModel.DataTransfer;

namespace CSC13001_my_shop_project.Presentation.Products;

public partial class ProductDetailViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IProductService _productService;
    private readonly IImageUploadService _imageUpload;
    private readonly ProductDetailArgs _args;
    private ProductDto? _currentProductDto;

    public ProductDetailViewModel(
        INavigator navigator,
        IProductService productService,
        IImageUploadService imageUpload,
        ProductDetailArgs args
    )
    {
        _navigator = navigator;
        _productService = productService;
        _imageUpload = imageUpload;
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

    [ObservableProperty]
    private bool isEditDialogOpen;

    [ObservableProperty]
    private CreateProductViewModel? editDialogViewModel;

    public ObservableCollection<OrderModel> RecentOrders { get; } = new();

    private async Task LoadProductAsync()
    {
        try
        {
            var dto = await _productService
                .GetByIdAsync(_args.GraphQlProductId)
                .ConfigureAwait(false);
            _currentProductDto = dto;
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
            _currentProductDto = null;
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
        if (IsEditDialogOpen)
            return;

        try
        {
            var dto = _currentProductDto;
            if (dto is null)
            {
                dto = await _productService
                    .GetByIdAsync(_args.GraphQlProductId);
                _currentProductDto = dto;
            }

            if (dto is null)
            {
                await ShowErrorDialogAsync("Product not found", "The product data could not be loaded.", null);
                return;
            }

            var vm = new CreateProductViewModel(
                _productService,
                _imageUpload,
                () =>
                {
                    IsEditDialogOpen = false;
                    EditDialogViewModel = null;
                },
                LoadProductAsync,
                dto);

            App.RunOnUIThread(() =>
            {
                EditDialogViewModel = vm;
                IsEditDialogOpen = true;
            });

            await vm.InitializeAsync();
        }
        catch (HttpRequestException)
        {
            await ShowErrorDialogAsync("Edit failed", "Cannot reach the server. Check your connection.", null);
        }
        catch (Exception ex)
        {
            await ShowErrorDialogAsync("Edit failed", ex.Message, null);
        }
    }

    [RelayCommand]
    private async Task DeleteProductAsync(FrameworkElement? root)
    {
        var xamlRoot = root?.XamlRoot;
        var displayName = string.IsNullOrWhiteSpace(Product?.Name)
            ? _args.GraphQlProductId
            : Product.Name;

        var confirmDialog = new DeleteProductDialog();
        if (xamlRoot is not null)
            confirmDialog.XamlRoot = xamlRoot;
        confirmDialog.SetProductName(displayName);
        await confirmDialog.ShowAsync();

        if (!confirmDialog.IsConfirmed)
            return;

        try
        {
            var ok = await _productService.DeleteAsync(_args.GraphQlProductId);
            if (!ok)
                throw new InvalidOperationException("deleteProduct returned false.");

            var successDialog = new DeleteProductSuccessDialog();
            if (xamlRoot is not null)
                successDialog.XamlRoot = xamlRoot;
            successDialog.SetProductName(displayName);
            await successDialog.ShowAsync();

            await _navigator.NavigateRouteAsync(this, "Products");
        }
        catch (GraphQlException ex)
        {
            var msg = ex.Errors.FirstOrDefault()?.Message ?? "Could not delete product.";
            var display = msg.StartsWith("Unauthenticated:", StringComparison.OrdinalIgnoreCase)
                ? "Sign in first. deleteProduct requires a JWT (Authorization: Bearer)."
                : msg.StartsWith("Unauthorized:", StringComparison.OrdinalIgnoreCase)
                    ? "Your role cannot delete products. Use an Admin account."
                    : msg;
            await ShowErrorDialogAsync("Delete failed", display, xamlRoot);
        }
        catch (HttpRequestException)
        {
            await ShowErrorDialogAsync(
                "Delete failed",
                "Cannot reach the server. Check your connection.",
                xamlRoot);
        }
        catch (Exception ex)
        {
            await ShowErrorDialogAsync("Delete failed", ex.Message, xamlRoot);
        }
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

    private static async Task ShowErrorDialogAsync(string title, string message, XamlRoot? xamlRoot = null)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = message,
            CloseButtonText = "OK",
        };
        if (xamlRoot is not null)
            dialog.XamlRoot = xamlRoot;
        await dialog.ShowAsync();
    }
}
