using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using Windows.Storage.Pickers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CSC13001_my_shop_project.Models;
using CSC13001_my_shop_project.Services;

namespace CSC13001_my_shop_project.Presentation.Products;

public partial class CreateProductViewModel : ObservableObject
{
    private readonly IProductService _productService;
    private readonly IImageUploadService _imageUpload;
    private readonly Action _onClose;
    private readonly Func<CreatedProductSummary?, Task>? _onCreated;

    public CreateProductViewModel(
        IProductService productService,
        IImageUploadService imageUpload,
        Action onClose,
        Func<CreatedProductSummary?, Task>? onCreated)
    {
        _productService = productService;
        _imageUpload = imageUpload;
        _onClose = onClose;
        _onCreated = onCreated;
    }

    [ObservableProperty]
    private string productName = "";

    [ObservableProperty]
    private string description = "";

    [ObservableProperty]
    private string sku = "";

    [ObservableProperty]
    private string supplier = "";

    [ObservableProperty]
    private string tagInput = "";

    [ObservableProperty]
    private string price = "";

    [ObservableProperty]
    private string priceCurrency = "USD";

    [ObservableProperty]
    private string comparePrice = "";

    [ObservableProperty]
    private string compareCurrency = "USD";

    [ObservableProperty]
    private string costPerItem = "";

    [ObservableProperty]
    private string costCurrency = "USD";

    [ObservableProperty]
    private bool trackQuantity = true;

    [ObservableProperty]
    private string quantity = "0";

    [ObservableProperty]
    private string weight = "";

    [ObservableProperty]
    private string weightUnit = "kg";

    [ObservableProperty]
    private string dimensions = "";

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string errorMessage = "";

    [ObservableProperty]
    private ObservableCollection<CategoryDto> categories = new();

    [ObservableProperty]
    private CategoryDto? selectedCategoryItem;

    [ObservableProperty]
    private ObservableCollection<string> tags = new();

    [ObservableProperty]
    private ObservableCollection<ImageItem> uploadedImages = new();

    public ObservableCollection<string> WeightUnits { get; } = new(["kg", "lb", "g", "oz"]);

    public string FormattedPrice =>
        string.IsNullOrWhiteSpace(Price) ? "—" : $"{PriceCurrency} {Price}";

    public string SummaryCategoryDisplay => SelectedCategoryItem?.Name ?? "—";

    public string SummaryNameDisplay => string.IsNullOrWhiteSpace(ProductName) ? "—" : ProductName;

    public string SummaryStockDisplay => TrackQuantity ? (string.IsNullOrWhiteSpace(Quantity) ? "—" : Quantity) : "—";

    public bool CanSubmitProduct => ComputeIsValid() && !IsBusy;

    public async Task InitializeAsync()
    {
        ErrorMessage = "";
        try
        {
            var list = await _productService.GetCategoriesAsync().ConfigureAwait(false);
            Categories = new ObservableCollection<CategoryDto>(list);
        }
        catch
        {
            ErrorMessage = "Could not load categories. Check the API server.";
        }
    }

    public void Reset()
    {
        ProductName = "";
        Description = "";
        Sku = "";
        Supplier = "";
        TagInput = "";
        Price = "";
        PriceCurrency = "USD";
        ComparePrice = "";
        CompareCurrency = "USD";
        CostPerItem = "";
        CostCurrency = "USD";
        TrackQuantity = true;
        Quantity = "0";
        Weight = "";
        WeightUnit = "kg";
        Dimensions = "";
        Tags.Clear();
        UploadedImages.Clear();
        SelectedCategoryItem = null;
        ErrorMessage = "";
        IsBusy = false;
        OnPropertyChanged(nameof(CanSubmitProduct));
        OnPropertyChanged(nameof(FormattedPrice));
        OnPropertyChanged(nameof(SummaryCategoryDisplay));
        OnPropertyChanged(nameof(SummaryNameDisplay));
        OnPropertyChanged(nameof(SummaryStockDisplay));
    }

    partial void OnProductNameChanged(string value) => NotifySummary();

    partial void OnSelectedCategoryItemChanged(CategoryDto? value) => NotifySummary();

    partial void OnPriceChanged(string value) => NotifySummary();

    partial void OnPriceCurrencyChanged(string value) => NotifySummary();

    partial void OnQuantityChanged(string value) => NotifySummary();

    partial void OnTrackQuantityChanged(bool value) => NotifySummary();

    partial void OnIsBusyChanged(bool value)
    {
        OnPropertyChanged(nameof(CanSubmitProduct));
    }

    private void NotifySummary()
    {
        OnPropertyChanged(nameof(FormattedPrice));
        OnPropertyChanged(nameof(SummaryCategoryDisplay));
        OnPropertyChanged(nameof(SummaryNameDisplay));
        OnPropertyChanged(nameof(SummaryStockDisplay));
        OnPropertyChanged(nameof(CanSubmitProduct));
    }

    private bool ComputeIsValid()
    {
        if (string.IsNullOrWhiteSpace(ProductName) || string.IsNullOrWhiteSpace(Sku) || SelectedCategoryItem is null)
            return false;
        if (!TryParsePrice(Price, out var pr) || pr <= 0)
            return false;
        if (TrackQuantity)
        {
            if (!int.TryParse(Quantity?.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var q) || q < 0)
                return false;
        }
        return true;
    }

    private static bool TryParsePrice(string? s, out int value)
    {
        value = 0;
        if (string.IsNullOrWhiteSpace(s))
            return false;
        var t = s.Trim().Replace(" ", "", StringComparison.Ordinal);
        if (decimal.TryParse(t, NumberStyles.Number, CultureInfo.InvariantCulture, out var d))
        {
            value = (int)Math.Round(d, MidpointRounding.AwayFromZero);
            return value > 0;
        }
        return false;
    }

    [RelayCommand]
    private void AddTag()
    {
        var t = TagInput.Trim();
        if (string.IsNullOrEmpty(t) || Tags.Contains(t))
            return;
        Tags.Add(t);
        TagInput = "";
    }

    [RelayCommand]
    private void RemoveTag(string? tag)
    {
        if (string.IsNullOrEmpty(tag))
            return;
        Tags.Remove(tag);
    }

    [RelayCommand]
    private void Cancel()
    {
        Reset();
        _onClose();
    }

    [RelayCommand]
    private async Task SubmitAsync()
    {
        if (!ComputeIsValid())
            return;

        IsBusy = true;
        ErrorMessage = "";
        OnPropertyChanged(nameof(CanSubmitProduct));
        try
        {
            var imageUrls = new List<string>();
            foreach (var img in UploadedImages.OrderByDescending(i => i.IsCover))
            {
                if (!string.IsNullOrEmpty(img.RemoteUrl))
                {
                    imageUrls.Add(img.RemoteUrl);
                    continue;
                }
                if (string.IsNullOrEmpty(img.LocalPath))
                    continue;
                await using (var fs = File.OpenRead(img.LocalPath))
                {
                    var name = string.IsNullOrEmpty(img.FileName) ? Path.GetFileName(img.LocalPath) : img.FileName;
                    var url = await _imageUpload.UploadImageAsync(fs, name).ConfigureAwait(false);
                    if (!string.IsNullOrEmpty(url))
                    {
                        img.RemoteUrl = url;
                        imageUrls.Add(url);
                    }
                }
            }

            TryParsePrice(Price, out var priceInt);
            var stock = 0;
            if (TrackQuantity)
                int.TryParse(Quantity.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out stock);

            var input = new CreateProductInput
            {
                Sku = Sku.Trim(),
                Name = ProductName.Trim(),
                Price = priceInt,
                Stock = stock,
                Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
                Images = imageUrls.Count > 0 ? imageUrls : null,
                Supplier = string.IsNullOrWhiteSpace(Supplier) ? null : Supplier.Trim(),
                CategoryId = SelectedCategoryItem!.CategoryId,
            };

            var summary = await _productService.CreateAsync(input).ConfigureAwait(false);

            if (_onCreated is not null)
                await _onCreated(summary).ConfigureAwait(false);

            Reset();
            _onClose();
        }
        catch (GraphqlException ex)
        {
            ErrorMessage = string.IsNullOrWhiteSpace(ex.Message)
                ? "Could not create product."
                : ex.Message;
        }
        catch (HttpRequestException)
        {
            ErrorMessage = "Cannot reach the server. Check your connection.";
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
            OnPropertyChanged(nameof(CanSubmitProduct));
        }
    }

    [RelayCommand]
    private async Task AddImagesFromPickerAsync()
    {
        try
        {
            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".webp");
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            var file = await picker.PickSingleFileAsync();
            if (file is null)
                return;
            var path = file.Path;
            var item = new ImageItem
            {
                LocalPath = path,
                FileName = file.Name,
                IsCover = UploadedImages.Count == 0,
            };
            UploadedImages.Add(item);
        }
        catch
        {
            ErrorMessage = "Could not open file picker.";
        }
    }

    [RelayCommand]
    private void SetCover(ImageItem? item)
    {
        if (item is null)
            return;
        foreach (var i in UploadedImages)
            i.IsCover = false;
        item.IsCover = true;
    }

    [RelayCommand]
    private void RemoveImage(ImageItem? item)
    {
        if (item is null)
            return;
        var wasCover = item.IsCover;
        UploadedImages.Remove(item);
        if (wasCover && UploadedImages.Count > 0)
            UploadedImages[0].IsCover = true;
    }
}