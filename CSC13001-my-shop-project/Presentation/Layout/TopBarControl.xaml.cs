using System.Windows.Input;
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

    private void OnSearchFocused(object sender, RoutedEventArgs e)
    {
        SearchFocusRing.BorderBrush = Application.Current.Resources["ShellAccentBrush"] as Brush;
    }

    private void OnSearchUnfocused(object sender, RoutedEventArgs e)
    {
        SearchFocusRing.BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Transparent);
    }
}
