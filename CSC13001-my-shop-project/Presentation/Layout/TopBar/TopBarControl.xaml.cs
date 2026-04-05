using System.Windows.Input;
using CommunityToolkit.Mvvm.Messaging;
using CSC13001_my_shop_project.Presentation.Dashboard;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace CSC13001_my_shop_project.Presentation.Layout;

public sealed partial class TopBarControl : UserControl
{
    public TopBarControl()
    {
        this.InitializeComponent();
        UpdateToggleVisibility();
    }

    public static readonly DependencyProperty IsSidebarExpandedProperty =
        DependencyProperty.Register(
            nameof(IsSidebarExpanded),
            typeof(bool),
            typeof(TopBarControl),
            new PropertyMetadata(true, OnSidebarStateChanged)
        );

    public static readonly DependencyProperty OpenSidebarCommandProperty =
        DependencyProperty.Register(
            nameof(OpenSidebarCommand),
            typeof(ICommand),
            typeof(TopBarControl),
            new PropertyMetadata(null)
        );

    public bool IsSidebarExpanded
    {
        get => (bool)GetValue(IsSidebarExpandedProperty);
        set => SetValue(IsSidebarExpandedProperty, value);
    }

    public ICommand? OpenSidebarCommand
    {
        get => (ICommand?)GetValue(OpenSidebarCommandProperty);
        set => SetValue(OpenSidebarCommandProperty, value);
    }

    private static void OnSidebarStateChanged(
        DependencyObject dependencyObject,
        DependencyPropertyChangedEventArgs args
    )
    {
        if (dependencyObject is TopBarControl control)
        {
            control.UpdateToggleVisibility();
        }
    }

    private void UpdateToggleVisibility()
    {
        OpenSidebarButton.Visibility = IsSidebarExpanded
            ? Visibility.Collapsed
            : Visibility.Visible;
    }

    private void SignOutButton_Click(object sender, RoutedEventArgs e)
    {
        ProfileFlyout.Hide();
        WeakReferenceMessenger.Default.Send(new NavigateToPageMessage("Login"));
    }

    private void ProfileFlyout_Opened(object sender, object e) {
        VisualStateManager.GoToState(ProfileButton, "FlyoutOpen", true);
    }

    private void ProfileFlyout_Closed(object sender, object e) {
        VisualStateManager.GoToState(ProfileButton, "FlyoutClosed", true);
    }
}
