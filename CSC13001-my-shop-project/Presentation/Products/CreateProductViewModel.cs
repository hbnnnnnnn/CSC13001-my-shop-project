using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using Microsoft.UI.Dispatching;
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
    private readonly Func<Task>? _onCreated;
    private readonly ProductDto? _editProduct;
    private readonly string? _editProductId;
    private readonly string? _initialCategoryId;
    private readonly string? _initialCategoryName;
    private readonly bool _isEditMode;

    public CreateProductViewModel(
        IProductService productService,
        IImageUploadService imageUpload,
        Action onClose,
        Func<Task>? onCreated,
        ProductDto? editProduct = null)
    {
        _productService = productService;
        _imageUpload = imageUpload;
        _onClose = onClose;
        _onCreated = onCreated;
        _editProduct = editProduct;
        _editProductId = string.IsNullOrWhiteSpace(editProduct?.ProductId) ? null : editProduct!.ProductId;
        _initialCategoryId = editProduct?.Category?.CategoryId;
        _initialCategoryName = editProduct?.Category?.Name;
        _isEditMode = editProduct is not null;

        if (_editProduct is not null)
            ApplyProductDefaults(_editProduct);
    }

    /// <summary>Invalidate in-flight AI suggestion when resetting or starting a new request.</summary>
    private int _aiSuggestionGeneration;

    private DispatcherQueueTimer? _topBannerDismissTimer;

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

    [ObservableProperty]
    private bool isAiGenerating;

    [ObservableProperty]
    private string topBannerMessage = "";

    public ObservableCollection<string> WeightUnits { get; } = new(["kg", "lb", "g", "oz"]);

    public bool IsEditMode => _isEditMode;

    public string DialogTitle => _isEditMode ? "EDIT PRODUCT" : "CREATE PRODUCT";

    public string SubmitLabel => _isEditMode ? "Save Changes" : "Add Product";

    public string FormattedPrice =>
        string.IsNullOrWhiteSpace(Price) ? "—" : $"{PriceCurrency} {Price}";

    public string SummaryCategoryDisplay => SelectedCategoryItem?.Name ?? "—";

    /// <summary>Shown in the category picker TextBox; empty when none selected so placeholder is visible.</summary>
    public string CategorySelectionDisplay => SelectedCategoryItem?.Name ?? "";

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
            if (!string.IsNullOrWhiteSpace(_initialCategoryId))
            {
                var picked = Categories.FirstOrDefault(c => c.CategoryId == _initialCategoryId);
                if (picked is not null)
                    SelectedCategoryItem = picked;
            }
            if (SelectedCategoryItem is null && !string.IsNullOrWhiteSpace(_initialCategoryName))
            {
                var picked = Categories.FirstOrDefault(c =>
                    string.Equals(c.Name, _initialCategoryName, StringComparison.OrdinalIgnoreCase));
                if (picked is not null)
                    SelectedCategoryItem = picked;
            }
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
        ClearTopBannerTimer();
        TopBannerMessage = "";
        IsBusy = false;
        IsAiGenerating = false;
        Interlocked.Increment(ref _aiSuggestionGeneration);
        OnPropertyChanged(nameof(CanSubmitProduct));
        OnPropertyChanged(nameof(FormattedPrice));
        OnPropertyChanged(nameof(SummaryCategoryDisplay));
        OnPropertyChanged(nameof(SummaryNameDisplay));
        OnPropertyChanged(nameof(SummaryStockDisplay));
    }

    partial void OnProductNameChanged(string value) => NotifySummary();

    partial void OnSelectedCategoryItemChanged(CategoryDto? value)
    {
        OnPropertyChanged(nameof(CategorySelectionDisplay));
        NotifySummary();
    }

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
        if (string.IsNullOrWhiteSpace(ProductName) || string.IsNullOrWhiteSpace(Sku))
            return false;
        if (!_isEditMode && SelectedCategoryItem is null)
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

    private void ApplyProductDefaults(ProductDto dto)
    {
        ProductName = dto.Name ?? "";
        Description = dto.Description ?? "";
        Sku = dto.Sku ?? "";
        Supplier = dto.Supplier ?? "";
        Price = dto.Price.ToString(CultureInfo.InvariantCulture);
        TrackQuantity = true;
        Quantity = dto.Stock.ToString(CultureInfo.InvariantCulture);

        Tags.Clear();
        if (!string.IsNullOrWhiteSpace(dto.Category?.Name))
            Tags.Add(dto.Category!.Name.Trim());
        if (!string.IsNullOrWhiteSpace(dto.Supplier) && !Tags.Contains(dto.Supplier.Trim()))
            Tags.Add(dto.Supplier.Trim());

        UploadedImages.Clear();
        if (dto.Images is { Count: > 0 })
        {
            var isCover = true;
            foreach (var url in dto.Images)
            {
                if (string.IsNullOrWhiteSpace(url))
                    continue;
                UploadedImages.Add(new ImageItem
                {
                    RemoteUrl = url,
                    FileName = Path.GetFileName(url),
                    IsCover = isCover,
                });
                isCover = false;
            }
        }

        NotifySummary();
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
    private async Task RequestAiSuggestionAsync()
    {
        if (UploadedImages.Count == 0)
        {
            ShowTopBannerTransient("Upload ảnh để sử dụng tính năng");
            return;
        }

        var gen = Interlocked.Increment(ref _aiSuggestionGeneration);
        IsAiGenerating = true;
        IsBusy = true;
        ErrorMessage = "";
        OnPropertyChanged(nameof(CanSubmitProduct));
        try
        {
            var url = await EnsureCoverImageUrlAsync().ConfigureAwait(false);
            if (gen != Volatile.Read(ref _aiSuggestionGeneration))
                return;
            if (string.IsNullOrEmpty(url))
            {
                App.RunOnUIThread(() =>
                    ErrorMessage = "Could not upload image for AI. Try again.");
                return;
            }

            var suggestion = await _productService
                .GenerateProductDetailsFromImageAsync(url)
                .ConfigureAwait(false);
            if (gen != Volatile.Read(ref _aiSuggestionGeneration))
                return;
            App.RunOnUIThread(() =>
            {
                if (suggestion is null)
                {
                    ErrorMessage = "AI returned no suggestion.";
                    return;
                }

                if (!string.IsNullOrWhiteSpace(suggestion.Name))
                    ProductName = suggestion.Name.Trim();
                if (!string.IsNullOrWhiteSpace(suggestion.Description))
                    Description = suggestion.Description.Trim();
                NotifySummary();
            });
        }
        catch (GraphQlException ex)
        {
            if (gen != Volatile.Read(ref _aiSuggestionGeneration))
                return;
            var msg = ex.Errors.FirstOrDefault()?.Message ?? "AI suggestion failed.";
            var display = msg.StartsWith("Unauthenticated:", StringComparison.OrdinalIgnoreCase)
                ? "Sign in as Admin or Sale to use AI (Bearer JWT)."
                : msg.StartsWith("Unauthorized:", StringComparison.OrdinalIgnoreCase)
                    ? "Your role cannot use AI suggestion."
                    : msg;
            App.RunOnUIThread(() => ErrorMessage = display);
        }
        catch (HttpRequestException)
        {
            if (gen != Volatile.Read(ref _aiSuggestionGeneration))
                return;
            App.RunOnUIThread(() =>
                ErrorMessage = "Cannot reach the server. Check your connection.");
        }
        catch (Exception ex)
        {
            if (gen != Volatile.Read(ref _aiSuggestionGeneration))
                return;
            App.RunOnUIThread(() => ErrorMessage = ex.Message);
        }
        finally
        {
            if (gen == Volatile.Read(ref _aiSuggestionGeneration))
            {
                App.RunOnUIThread(() =>
                {
                    IsAiGenerating = false;
                    IsBusy = false;
                    OnPropertyChanged(nameof(CanSubmitProduct));
                });
            }
        }
    }

    private void ClearTopBannerTimer()
    {
        if (_topBannerDismissTimer is null)
            return;
        _topBannerDismissTimer.Tick -= OnTopBannerDismissTick;
        _topBannerDismissTimer.Stop();
        _topBannerDismissTimer = null;
    }

    private void OnTopBannerDismissTick(DispatcherQueueTimer sender, object args)
    {
        sender.Tick -= OnTopBannerDismissTick;
        sender.Stop();
        TopBannerMessage = "";
        _topBannerDismissTimer = null;
    }

    private void ShowTopBannerTransient(string message)
    {
        App.RunOnUIThread(() =>
        {
            ClearTopBannerTimer();
            TopBannerMessage = message;
            if (string.IsNullOrEmpty(message))
                return;

            var dq = DispatcherQueue.GetForCurrentThread() ?? App.UiThreadDispatcher;
            if (dq is null)
                return;

            _topBannerDismissTimer = dq.CreateTimer();
            _topBannerDismissTimer.Interval = TimeSpan.FromSeconds(3);
            _topBannerDismissTimer.IsRepeating = false;
            _topBannerDismissTimer.Tick += OnTopBannerDismissTick;
            _topBannerDismissTimer.Start();
        });
    }

    /// <summary>Cover image (or first); uploads locally if needed so the server can fetch a public URL.</summary>
    private async Task<string?> EnsureCoverImageUrlAsync()
    {
        var img = UploadedImages.OrderByDescending(i => i.IsCover).FirstOrDefault();
        if (img is null)
            return null;
        if (!string.IsNullOrEmpty(img.RemoteUrl))
            return img.RemoteUrl;
        if (string.IsNullOrEmpty(img.LocalPath))
            return null;
        await using var fs = File.OpenRead(img.LocalPath);
        var name = string.IsNullOrEmpty(img.FileName) ? Path.GetFileName(img.LocalPath) : img.FileName;
        var url = await _imageUpload.UploadImageAsync(fs, name).ConfigureAwait(false);
        if (!string.IsNullOrEmpty(url))
            img.RemoteUrl = url;
        return url;
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

            if (_isEditMode)
            {
                if (string.IsNullOrWhiteSpace(_editProductId))
                    throw new InvalidOperationException("Missing product id for update.");

                var input = new UpdateProductInput
                {
                    Sku = Sku.Trim(),
                    Name = ProductName.Trim(),
                    Price = priceInt,
                    Stock = stock,
                    Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
                    Images = imageUrls,
                    Supplier = string.IsNullOrWhiteSpace(Supplier) ? null : Supplier.Trim(),
                    CategoryId = SelectedCategoryItem?.CategoryId,
                };

                await _productService.UpdateAsync(_editProductId, input).ConfigureAwait(false);
            }
            else
            {
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

                await _productService.CreateAsync(input).ConfigureAwait(false);
            }

            if (_onCreated is not null)
                await _onCreated().ConfigureAwait(false);

            App.RunOnUIThread(() =>
            {
                Reset();
                _onClose();
            });
        }
        catch (GraphQlException ex)
        {
            var msg = ex.Errors.FirstOrDefault()?.Message
                ?? (_isEditMode ? "Could not update product." : "Could not create product.");
            var display = msg.StartsWith("Unauthenticated:", StringComparison.OrdinalIgnoreCase)
                ? (_isEditMode
                    ? "Sign in first. updateProduct requires a JWT (Authorization: Bearer) — see backend testGraphQL.md §1 and §5.1."
                    : "Sign in first. createProduct requires a JWT (Authorization: Bearer) — see backend testGraphQL.md §1 and §5.1.")
                : msg.StartsWith("Unauthorized:", StringComparison.OrdinalIgnoreCase)
                    ? (_isEditMode
                        ? "Your role cannot update products. Use an Admin or Sale account."
                        : "Your role cannot create products. Use an Admin or Sale account.")
                    : msg;
            App.RunOnUIThread(() => ErrorMessage = display);
        }
        catch (HttpRequestException)
        {
            App.RunOnUIThread(() =>
                ErrorMessage = "Cannot reach the server. Check your connection.");
        }
        catch (Exception ex)
        {
            App.RunOnUIThread(() => ErrorMessage = ex.Message);
        }
        finally
        {
            App.RunOnUIThread(() =>
            {
                IsBusy = false;
                OnPropertyChanged(nameof(CanSubmitProduct));
            });
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
