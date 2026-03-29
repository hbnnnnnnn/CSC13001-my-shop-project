namespace CSC13001_my_shop_project.Presentation;

using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using CSC13001_my_shop_project.Presentation.Dashboard;
using CSC13001_my_shop_project.Presentation.OrderList;
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

        _vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(ShellViewModel.SelectedSidebarItem))
                SyncContentVisibility();
        };
        
        this.Loaded += (_, _) => SyncContentVisibility();
        Loaded += OnShellLoaded;
    }

    public ContentControl ContentControl => MainContent;

    private async void SyncContentVisibility()
    {
        MainContent.Visibility = Visibility.Visible;
        if (string.IsNullOrEmpty(_vm.SelectedSidebarItem)) return;

        switch (_vm.SelectedSidebarItem)
        {
            case "Dashboard":
                await MainContent.Navigator().NavigateViewModelAsync<DashboardViewModel>(this);
                break;
            case "Orders":
                await MainContent.Navigator().NavigateViewModelAsync<OrderListViewModel>(this);
                break;
            default:
                await MainContent.Navigator().NavigateRouteAsync(this, _vm.SelectedSidebarItem);
                break;
        }
    private void OnShellLoaded(object sender, RoutedEventArgs e)
    {
        ShellViewModel.AttachNavigatorResolver(() => this.Navigator() ?? MainContent?.Navigator());
    }
}
