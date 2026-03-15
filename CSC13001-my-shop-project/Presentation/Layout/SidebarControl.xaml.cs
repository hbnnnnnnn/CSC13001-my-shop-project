using System.Linq;
using System.Windows.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;

namespace CSC13001_my_shop_project.Presentation.Layout;

public sealed partial class SidebarControl : UserControl
{
    private bool _hasLoaded;

    public SidebarControl()
    {
        this.InitializeComponent();
        foreach (var item in GetNavItems())
            item.ItemSelected += OnNavItemSelected;
        RootBorder.Visibility = IsExpanded ? Visibility.Visible : Visibility.Collapsed;
        UpdateSelectionState();
        _hasLoaded = true;
    }

    public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register(
        nameof(IsExpanded),
        typeof(bool),
        typeof(SidebarControl),
        new PropertyMetadata(true, OnVisualPropertyChanged)
    );

    public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
        nameof(SelectedItem),
        typeof(string),
        typeof(SidebarControl),
        new PropertyMetadata("Dashboard", OnVisualPropertyChanged)
    );

    public static readonly DependencyProperty CloseSidebarCommandProperty =
        DependencyProperty.Register(
            nameof(CloseSidebarCommand),
            typeof(ICommand),
            typeof(SidebarControl),
            new PropertyMetadata(null)
        );

    public static readonly DependencyProperty SelectItemCommandProperty =
        DependencyProperty.Register(
            nameof(SelectItemCommand),
            typeof(ICommand),
            typeof(SidebarControl),
            new PropertyMetadata(null)
        );

    public bool IsExpanded
    {
        get => (bool)GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    public string SelectedItem
    {
        get => (string)GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public ICommand? CloseSidebarCommand
    {
        get => (ICommand?)GetValue(CloseSidebarCommandProperty);
        set => SetValue(CloseSidebarCommandProperty, value);
    }

    public ICommand? SelectItemCommand
    {
        get => (ICommand?)GetValue(SelectItemCommandProperty);
        set => SetValue(SelectItemCommandProperty, value);
    }

    private static void OnVisualPropertyChanged(
        DependencyObject dependencyObject,
        DependencyPropertyChangedEventArgs args
    )
    {
        if (dependencyObject is SidebarControl control)
        {
            control.UpdateSidebarState();
            control.UpdateSelectionState();
        }
    }

    private void OnCloseSidebarClicked(object sender, RoutedEventArgs e)
    {
        if (CloseSidebarCommand is null || !CloseSidebarCommand.CanExecute(null))
        {
            Console.WriteLine("CloseSidebarCommand cannot execute");
            return;
        }

        CloseSidebarCommand?.Execute(null);
        IsExpanded = false;
    }

    private void OnNavItemSelected(object? sender, string key) => SetSelection(key);

    private void SetSelection(string item)
    {
        SelectedItem = item;
        SelectItemCommand?.Execute(item);
        UpdateSelectionState();
    }

    private void UpdateSidebarState()
    {
        if (!_hasLoaded)
            return;

        var easeOut = new CubicEase { EasingMode = EasingMode.EaseOut };
        var easeIn = new CubicEase { EasingMode = EasingMode.EaseIn };

        if (IsExpanded)
        {
            RootBorder.Opacity = 0;
            SidebarTranslate.X = -16;
            RootBorder.Visibility = Visibility.Visible;

            var sb = new Storyboard();

            var opacityAnim = new DoubleAnimation
            {
                To = 1,
                Duration = new Duration(TimeSpan.FromMilliseconds(250)),
                EasingFunction = easeOut,
            };
            Storyboard.SetTarget(opacityAnim, RootBorder);
            Storyboard.SetTargetProperty(opacityAnim, "Opacity");

            var translateAnim = new DoubleAnimation
            {
                To = 0,
                Duration = new Duration(TimeSpan.FromMilliseconds(280)),
                EasingFunction = easeOut,
            };
            Storyboard.SetTarget(translateAnim, SidebarTranslate);
            Storyboard.SetTargetProperty(translateAnim, "X");

            sb.Children.Add(opacityAnim);
            sb.Children.Add(translateAnim);
            sb.Begin();
        }
        else
        {
            var sb = new Storyboard();

            var opacityAnim = new DoubleAnimation
            {
                To = 0,
                Duration = new Duration(TimeSpan.FromMilliseconds(180)),
                EasingFunction = easeIn,
            };
            Storyboard.SetTarget(opacityAnim, RootBorder);
            Storyboard.SetTargetProperty(opacityAnim, "Opacity");

            var translateAnim = new DoubleAnimation
            {
                To = -16,
                Duration = new Duration(TimeSpan.FromMilliseconds(200)),
                EasingFunction = easeIn,
            };
            Storyboard.SetTarget(translateAnim, SidebarTranslate);
            Storyboard.SetTargetProperty(translateAnim, "X");

            sb.Children.Add(opacityAnim);
            sb.Children.Add(translateAnim);
            sb.Completed += (_, _) =>
            {
                RootBorder.Visibility = Visibility.Collapsed;
                RootBorder.Opacity = 1;
                SidebarTranslate.X = 0;
            };
            sb.Begin();
        }
    }

    private void UpdateSelectionState()
    {
        foreach (var item in GetNavItems())
            item.IsSelected = string.Equals(
                item.Key,
                SelectedItem,
                StringComparison.OrdinalIgnoreCase
            );
    }

    private IEnumerable<SidebarNavItem> GetNavItems() =>
        NavItemsPanel.Children.OfType<SidebarNavItem>();
}
