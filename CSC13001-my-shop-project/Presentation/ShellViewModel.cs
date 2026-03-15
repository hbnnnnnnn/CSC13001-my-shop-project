namespace CSC13001_my_shop_project.Presentation;

using CommunityToolkit.Mvvm.Messaging;
using CSC13001_my_shop_project.Presentation.Dashboard;

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

        // Listen for navigation requests broadcast by child ViewModels (e.g. Dashboard)
        WeakReferenceMessenger.Default.Register<NavigateToPageMessage>(this, (_, msg) =>
        {
            SelectSidebarItemCommand.Execute(msg.PageKey);
        });
    }

    public IRelayCommand CloseSidebarCommand { get; }

    public IRelayCommand OpenSidebarCommand { get; }

    public IRelayCommand<string> SelectSidebarItemCommand { get; }

    public string TestString { get; set; } = "Hello from ShellViewModel!";
}
