namespace CSC13001_my_shop_project.Presentation;

public sealed partial class Shell : UserControl, IContentControlProvider
{
    private readonly ShellViewModel _vm;

    public Shell()
    {
        this.InitializeComponent();
        _vm = new ShellViewModel();
        DataContext = _vm;

        // Show content only for the Dashboard tab; hide for all others (not yet implemented)
        _vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(ShellViewModel.SelectedSidebarItem))
                SyncContentVisibility();
        };
    }

    public ContentControl ContentControl => MainContent;

    private void SyncContentVisibility()
    {
        MainContent.Visibility = _vm.SelectedSidebarItem == "Dashboard"
            ? Visibility.Visible
            : Visibility.Collapsed;
    }
}
