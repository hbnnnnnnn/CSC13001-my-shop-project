using CommunityToolkit.Mvvm.Messaging;
using CSC13001_my_shop_project.Presentation.Dashboard;
using Microsoft.UI.Xaml;
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
        Loaded += OnShellLoaded;

        // Listen for chrome visibility messages from pages.
        WeakReferenceMessenger.Default.Register<ChromeVisibilityMessage>(
            this,
            (r, msg) =>
            {
                var visibility = msg.ShowChrome ? Visibility.Visible : Visibility.Collapsed;
                Sidebar.Visibility = visibility;
                TopBar.Visibility = visibility;
            }
        );
    }

    public ContentControl ContentControl => MainContent;

    private void OnShellLoaded(object sender, RoutedEventArgs e)
    {
        ShellViewModel.AttachNavigatorResolver(() => this.Navigator() ?? MainContent?.Navigator());
    }
}
