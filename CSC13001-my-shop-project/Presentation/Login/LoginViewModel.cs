using CommunityToolkit.Mvvm.Messaging;
using CSC13001_my_shop_project.Presentation.Dashboard;
using CSC13001_my_shop_project.Services;

namespace CSC13001_my_shop_project.Presentation.Login;

public partial class LoginViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly AuthService _auth;

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private bool rememberMe;

    [ObservableProperty]
    private bool isPasswordVisible;

    [ObservableProperty]
    private string? errorMessage;

    [ObservableProperty]
    private bool isLoading;

    public LoginViewModel(INavigator navigator, AuthService auth)
    {
        _navigator = navigator;
        _auth = auth;
        GoToServerConfiguration = new AsyncRelayCommand(GoToServerConfigurationAsync);
        SignIn = new AsyncRelayCommand(SignInAsync);
    }

    public IAsyncRelayCommand GoToServerConfiguration { get; }

    public IAsyncRelayCommand SignIn { get; }

    private async Task GoToServerConfigurationAsync()
    {
        WeakReferenceMessenger.Default.Send(new NavigateToPageMessage("ServerConfiguration"));
        await Task.CompletedTask;
    }

    private async Task SignInAsync()
    {
        ErrorMessage = null;
        IsLoading = true;

        if (Email == "" && Password == "")
        {
            ErrorMessage = "Please fill in your email and password.";
            IsLoading = false;
            return;
        }
        else if (Email == "")
        {
            ErrorMessage = "Please fill in your email.";
            IsLoading = false;
            return;
        }
        else if (Password == "")
        {
            ErrorMessage = "Please fill in your password.";
            IsLoading = false;
            return;
        }

        try
        {
            var account = await _auth.LoginAsync(Email, Password, RememberMe);

            // Navigate to dashboard on success
            WeakReferenceMessenger.Default.Send(new NavigateToPageMessage("Dashboard"));
        }
        catch (GraphqlException ex)
        {
            // Backend errors: "Invalid username", "Invalid password", "Too many attempts..."
            ErrorMessage = ex.Message;
        }
        catch (HttpRequestException)
        {
            ErrorMessage = "Cannot connect to server. Check your connection settings.";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"An unexpected error occurred: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
