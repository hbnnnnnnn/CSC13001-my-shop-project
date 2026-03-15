namespace CSC13001_my_shop_project.Presentation;

public partial class ShellViewModel : ObservableObject
{
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
            if (!string.IsNullOrWhiteSpace(item))
            {
                SelectedSidebarItem = item;
            }
        });
    }

    public IRelayCommand CloseSidebarCommand { get; }

    public IRelayCommand OpenSidebarCommand { get; }

    public IRelayCommand<string> SelectSidebarItemCommand { get; }

    public string TestString { get; set; } = "Hello from ShellViewModel!";
}
