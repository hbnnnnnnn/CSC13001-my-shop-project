namespace CSC13001_my_shop_project.Presentation;

using CommunityToolkit.Mvvm.Messaging;
using CSC13001_my_shop_project.Presentation.Dashboard;
using CSC13001_my_shop_project.Services;
using Microsoft.Extensions.DependencyInjection;
using Uno.Extensions.Navigation;

public partial class ShellViewModel : ObservableObject
{
    /// <summary>
    /// Set from <see cref="Shell"/> when loaded so navigation uses the UI-attached <see cref="INavigator"/>
    /// (root <c>IServiceProvider.GetService&lt;INavigator&gt;()</c> is often null).
    /// </summary>
    private static Func<INavigator?>? _navigatorResolver;

    internal static void AttachNavigatorResolver(Func<INavigator?> resolver) =>
        _navigatorResolver = resolver;

    private AppStateService? _appStateService;
    private AppStateService AppState =>
        _appStateService ??= App.AppHost?.Services.GetRequiredService<AppStateService>()!;

    /// <summary>Routes that should hide the shell chrome (sidebar + top bar).</summary>
    private static readonly HashSet<string> ChromelessRoutes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Login",
        "ServerConfiguration",
    };

    private readonly HashSet<string> _sidebarRoutes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Dashboard",
        "Orders",
        "Products",
    };

    [ObservableProperty]
    private string _userName = string.Empty;

    [ObservableProperty]
    private string _userEmail = string.Empty;

    [ObservableProperty]
    private bool _isSidebarExpanded = true;

    [ObservableProperty]
    private string _selectedSidebarItem = "Dashboard";

    [ObservableProperty]
    private bool _isChromeVisible;

    public ShellViewModel()
    {
        CloseSidebarCommand = new RelayCommand(() =>
        {
            AppState.Clear();
            IsSidebarExpanded = false;
        });
        OpenSidebarCommand = new RelayCommand(() =>
        {
            IsSidebarExpanded = true;
        });
        SelectSidebarItemCommand = new RelayCommand<string>(item =>
        {
            if (string.IsNullOrWhiteSpace(item))
                return;
            SelectedSidebarItem = item;
            _ = Navigate(item);
        });

        SignOutCommand = new AsyncRelayCommand(SignOutAsync);

        WeakReferenceMessenger.Default.Register<NavigateToPageMessage>(
            this,
            (r, msg) =>
            {
                if (string.IsNullOrWhiteSpace(msg.PageKey) || r is not ShellViewModel shell)
                {
                    return;
                }

                var route = msg.PageKey;

                _ = Navigate(route);
            }
        );
    }

    public IRelayCommand CloseSidebarCommand { get; }

    public IRelayCommand OpenSidebarCommand { get; }

    public IRelayCommand<string> SelectSidebarItemCommand { get; }

    public IAsyncRelayCommand SignOutCommand { get; }

    public string TestString { get; set; } = "Hello from ShellViewModel!";

    /// <summary>
    /// Called by the Shell whenever a navigation occurs so we can show/hide chrome.
    /// </summary>
    internal void UpdateChromeVisibility(string? routeName)
    {
        IsChromeVisible = !string.IsNullOrEmpty(routeName) && !ChromelessRoutes.Contains(routeName);
    }

    private async Task Navigate(string route)
    {
        var nav =
            _navigatorResolver?.Invoke()
            ?? global::CSC13001_my_shop_project.App.AppHost?.Services.GetService<INavigator>();
        if (nav is null)
            return;
        try
        {
            switch (route)
            {
                case "Dashboard":
                    await nav.NavigateRouteAsync(this, "Dashboard");
                    break;
                case "Orders":
                    await nav.NavigateRouteAsync(this, "Orders");
                    break;
                case "Products":
                    await nav.NavigateRouteAsync(this, "Products");
                    break;
                case "Login":
                    await nav.NavigateRouteAsync(this, "Login");
                    break;
                case "ServerConfiguration":
                    await nav.NavigateRouteAsync(this, "ServerConfiguration");
                    break;
                default:
                    return;
            }

            if (_sidebarRoutes.Contains(route))
                SelectedSidebarItem = route;

            AppState.LastPage = route;

            UpdateChromeVisibility(route);

            // Refresh user info when entering a chrome-visible page
            if (!ChromelessRoutes.Contains(route))
                RefreshUserInfo();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
        }
    }

    /// <summary>
    /// Refreshes the displayed user name and email from the current auth session.
    /// </summary>
    private void RefreshUserInfo()
    {
        var auth = App.AppHost?.Services.GetService<AuthService>();
        if (auth?.CurrentAccount is { } acct)
        {
            UserName = acct.FullName;
            UserEmail = acct.Username;
        }
    }

    /// <summary>
    /// Signs the user out, clears the session, and navigates to the login page.
    /// </summary>
    private async Task SignOutAsync()
    {
        var auth = App.AppHost?.Services.GetService<AuthService>();
        if (auth is not null)
            await auth.LogoutAsync();

        AppState.Clear();

        UserName = string.Empty;
        UserEmail = string.Empty;

        WeakReferenceMessenger.Default.Send(new NavigateToPageMessage("Login"));
    }
}
