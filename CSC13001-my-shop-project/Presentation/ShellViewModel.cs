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

    internal static void AttachNavigatorResolver(Func<INavigator?> resolver) => _navigatorResolver = resolver;

    [ObservableProperty]
    private bool isSidebarExpanded = true;

    [ObservableProperty]
    private string selectedSidebarItem = "Dashboard";

    public ShellViewModel()
    {
        CloseSidebarCommand = new RelayCommand(() =>
        {
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
            _ = NavigateForSidebarAsync(item);
        });

        WeakReferenceMessenger.Default.Register<NavigateToPageMessage>(this, (r, msg) =>
        {
            if (string.IsNullOrWhiteSpace(msg.PageKey) || r is not ShellViewModel shell)
                return;
            shell.SelectedSidebarItem = msg.PageKey;
            _ = shell.NavigateForSidebarAsync(msg.PageKey);
        });
    }

    public IRelayCommand CloseSidebarCommand { get; }

    public IRelayCommand OpenSidebarCommand { get; }

    public IRelayCommand<string> SelectSidebarItemCommand { get; }

    public string TestString { get; set; } = "Hello from ShellViewModel!";

    private async Task NavigateForSidebarAsync(string key)
    {
        var nav = _navigatorResolver?.Invoke()
            ?? global::CSC13001_my_shop_project.App.AppHost?.Services.GetService<INavigator>();
        if (nav is null)
            return;
        try
        {
            switch (key)
            {
                case "Dashboard":
                    await nav.NavigateRouteAsync(this, "Dashboard");
                    break;
                case "Products":
                    await nav.NavigateRouteAsync(this, "Products");
                    break;
                default:
                    return;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
        }
    }
}
