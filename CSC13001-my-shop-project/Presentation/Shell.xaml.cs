using CommunityToolkit.Mvvm.Messaging;
using CSC13001_my_shop_project;
using CSC13001_my_shop_project.Presentation.Dashboard;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Uno.Extensions.Navigation;

namespace CSC13001_my_shop_project.Presentation;

public sealed partial class Shell : UserControl, IContentControlProvider
{
    private readonly ShellViewModel _vm;

    public Shell()
    {
        this.InitializeComponent();
        _vm = new ShellViewModel();
        DataContext = _vm;

        _vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(ShellViewModel.SelectedSidebarItem))
                SyncContentVisibility();
        };

        Loaded += OnShellLoaded;
    }

    public ContentControl ContentControl => MainContent;

    private async void OnShellLoaded(object sender, RoutedEventArgs e)
    {
        ShellViewModel.AttachNavigatorResolver(() => this.Navigator() ?? MainContent?.Navigator());
        SyncContentVisibility();

        // First NavigateToPageMessage can run before Loaded, so INavigator was null and nothing was shown.
        if (MainContent.Content is null)
        {
            var route = await App.ResolveStartupRouteAsync();
            WeakReferenceMessenger.Default.Send(new NavigateToPageMessage(route));
        }
    }

    private void SyncContentVisibility()
    {
        _vm.UpdateChromeVisibility(_vm.SelectedSidebarItem);
    }
}
