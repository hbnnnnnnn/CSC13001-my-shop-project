using CommunityToolkit.Mvvm.Messaging;
using CSC13001_my_shop_project.Presentation.Dashboard;

namespace CSC13001_my_shop_project.Presentation.Login;

public partial class LoginViewModel : ObservableObject
{
    private readonly INavigator _navigator;

    [ObservableProperty]
    private string email = "admin@luminahaven.com";

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private bool rememberMe;

    [ObservableProperty]
    private bool isPasswordVisible;

    public LoginViewModel(INavigator navigator)
    {
        _navigator = navigator;
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
        WeakReferenceMessenger.Default.Send(new NavigateToPageMessage("Dashboard"));
        await Task.CompletedTask;
    }
}
