using System.Linq;
using System.Net.Http;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CSC13001_my_shop_project.Services;

namespace CSC13001_my_shop_project.Presentation.Products;

public partial class CreateCategoryViewModel : ObservableObject
{
    private readonly IProductService _productService;
    private readonly Action _onClose;
    private readonly Func<Task>? _onCreated;

    public CreateCategoryViewModel(
        IProductService productService,
        Action onClose,
        Func<Task>? onCreated)
    {
        _productService = productService;
        _onClose = onClose;
        _onCreated = onCreated;
    }

    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string description = string.Empty;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public bool CanSubmit => !IsBusy && !string.IsNullOrWhiteSpace(Name);

    partial void OnNameChanged(string value) => OnPropertyChanged(nameof(CanSubmit));

    partial void OnIsBusyChanged(bool value) => OnPropertyChanged(nameof(CanSubmit));

    [RelayCommand]
    public async Task SubmitAsync()
    {
        if (string.IsNullOrWhiteSpace(Name))
            return;

        IsBusy = true;
        ErrorMessage = "";
        try
        {
            await _productService
                .CreateCategoryAsync(Name.Trim(), string.IsNullOrWhiteSpace(Description) ? null : Description.Trim())
                .ConfigureAwait(false);

            if (_onCreated is not null)
                await _onCreated().ConfigureAwait(false);

            App.RunOnUIThread(() =>
            {
                Name = string.Empty;
                Description = string.Empty;
                _onClose();
            });
        }
        catch (GraphQlException ex)
        {
            var msg = ex.Errors.FirstOrDefault()?.Message ?? "Could not create category.";
            var display = msg.StartsWith("Unauthenticated:", StringComparison.OrdinalIgnoreCase)
                ? "Sign in first. createCategory requires a JWT (Authorization: Bearer)."
                : msg.StartsWith("Unauthorized:", StringComparison.OrdinalIgnoreCase)
                    ? "Your role cannot create categories. Use an Admin account."
                    : msg;
            App.RunOnUIThread(() => ErrorMessage = display);
        }
        catch (HttpRequestException)
        {
            App.RunOnUIThread(() => ErrorMessage = "Cannot reach the server. Check your connection.");
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
                OnPropertyChanged(nameof(CanSubmit));
                OnPropertyChanged(nameof(HasError));
            });
        }
    }
}
