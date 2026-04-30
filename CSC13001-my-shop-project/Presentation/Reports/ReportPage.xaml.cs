using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace CSC13001_my_shop_project.Presentation.Reports;

public sealed partial class ReportPage : Page
{
    private bool _hasTriggeredInitialRefresh;

    public ReportPage()
    {
        this.InitializeComponent();
        DataContextChanged += ReportPage_DataContextChanged;
    }

    private async void ReportPage_OnLoaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        await TryTriggerInitialRefreshAsync();
    }

    private async void ReportPage_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        await TryTriggerInitialRefreshAsync();
    }

    private async Task TryTriggerInitialRefreshAsync()
    {
        if (_hasTriggeredInitialRefresh || DataContext is not ReportViewModel vm)
        {
            return;
        }

        _hasTriggeredInitialRefresh = true;
        await vm.RefreshCommand.ExecuteAsync(null);
    }
}
