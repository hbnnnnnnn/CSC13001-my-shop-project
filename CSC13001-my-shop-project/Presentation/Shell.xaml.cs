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
    }

    public ContentControl ContentControl => MainContent;

    private void OnShellLoaded(object sender, RoutedEventArgs e)
    {
        ShellViewModel.AttachNavigatorResolver(() => this.Navigator() ?? MainContent?.Navigator());
    }
}
