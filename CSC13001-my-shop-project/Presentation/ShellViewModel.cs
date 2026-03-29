namespace CSC13001_my_shop_project.Presentation;

using CommunityToolkit.Mvvm.Messaging;
using CSC13001_my_shop_project.Presentation.Dashboard;
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
        "Products",
    };

    [ObservableProperty]
    private bool isSidebarExpanded = true;

    [ObservableProperty]
    private string selectedSidebarItem = "Dashboard";

    [ObservableProperty]
    private bool isChromeVisible;

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

        WeakReferenceMessenger.Default.Register<NavigateToPageMessage>(
            this,
            (r, msg) =>
            {
                if (string.IsNullOrWhiteSpace(msg.PageKey) || r is not ShellViewModel shell)
                    return;

                var route = msg.PageKey;

                _ = Navigate(route);
            }
        );
    }

    public IRelayCommand CloseSidebarCommand { get; }

    public IRelayCommand OpenSidebarCommand { get; }

    public IRelayCommand<string> SelectSidebarItemCommand { get; }

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
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
        }
    }
}
