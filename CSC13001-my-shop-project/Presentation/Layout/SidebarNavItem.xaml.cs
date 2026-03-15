using System.Linq;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using PathShape = Microsoft.UI.Xaml.Shapes.Path;

namespace CSC13001_my_shop_project.Presentation.Layout;

public sealed partial class SidebarNavItem : UserControl
{
    public event EventHandler<string>? ItemSelected;

    private bool _isHovered;

    public SidebarNavItem()
    {
        this.InitializeComponent();
    }

    public static readonly DependencyProperty KeyProperty = DependencyProperty.Register(
        nameof(Key),
        typeof(string),
        typeof(SidebarNavItem),
        new PropertyMetadata(string.Empty)
    );

    public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
        nameof(Label),
        typeof(string),
        typeof(SidebarNavItem),
        new PropertyMetadata(string.Empty, OnLabelChanged)
    );

    public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(
        nameof(IsSelected),
        typeof(bool),
        typeof(SidebarNavItem),
        new PropertyMetadata(false, OnVisualPropertyChanged)
    );

    public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register(
        nameof(IsExpanded),
        typeof(bool),
        typeof(SidebarNavItem),
        new PropertyMetadata(true, OnVisualPropertyChanged)
    );

    public static readonly DependencyProperty IconContentProperty = DependencyProperty.Register(
        nameof(IconContent),
        typeof(object),
        typeof(SidebarNavItem),
        new PropertyMetadata(null, OnIconContentChanged)
    );

    public string Key
    {
        get => (string)GetValue(KeyProperty);
        set => SetValue(KeyProperty, value);
    }

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    public bool IsExpanded
    {
        get => (bool)GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    public object? IconContent
    {
        get => GetValue(IconContentProperty);
        set => SetValue(IconContentProperty, value);
    }

    private static void OnLabelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SidebarNavItem control)
            control.LabelText.Text = e.NewValue as string ?? string.Empty;
    }

    private static void OnIconContentChanged(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e
    )
    {
        if (d is SidebarNavItem control)
        {
            control.IconHostPresenter.Content = e.NewValue;
            control.UpdateState();
        }
    }

    private static void OnVisualPropertyChanged(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e
    )
    {
        if (d is SidebarNavItem control)
            control.UpdateState();
    }

    private void OnItemClicked(object sender, RoutedEventArgs e) => ItemSelected?.Invoke(this, Key);

    private void OnPointerEntered(object sender, PointerRoutedEventArgs e)
    {
        _isHovered = true;
        UpdateState();
    }

    private void OnPointerExited(object sender, PointerRoutedEventArgs e)
    {
        _isHovered = false;
        UpdateState();
    }

    private void UpdateState()
    {
        var activeBackground =
            Application.Current.Resources["ShellNavActiveBackgroundBrush"] as Brush;
        var hoverBackground =
            Application.Current.Resources["ShellNavHoverBackgroundBrush"] as Brush;
        var activeIcon = Application.Current.Resources["ShellAccentBrush"] as Brush;
        var inactiveIcon = Application.Current.Resources["ShellIconMutedBrush"] as Brush;
        var activeText = Application.Current.Resources["ShellTextStrongBrush"] as Brush;
        var inactiveText = Application.Current.Resources["ShellTextMutedBrush"] as Brush;

        bool isHovered = _isHovered && !IsSelected;

        var shadowOpacity = IsSelected ? 0.1 : 0.0;
        foreach (var shadow in NavShadowContainer.Shadows)
            shadow.Opacity = shadowOpacity;

        HoverSurface.Background = hoverBackground;
        HoverSurface.Opacity = isHovered ? 1d : 0d;

        ActiveSurface.Background = activeBackground;
        ActiveSurface.Opacity = IsSelected ? 1d : 0d;
        ActiveGradient.Opacity = IsSelected ? 1d : 0d;

        Marker.Visibility = IsSelected ? Visibility.Visible : Visibility.Collapsed;
        Chevron.Visibility = IsSelected && IsExpanded ? Visibility.Visible : Visibility.Collapsed;
        LabelText.Visibility = IsExpanded ? Visibility.Visible : Visibility.Collapsed;

        LabelText.Foreground = IsSelected ? activeText : inactiveText;
        LabelText.FontWeight = IsSelected ? FontWeights.Medium : FontWeights.Normal;

        var iconStroke = IsSelected ? activeIcon : inactiveIcon;
        var iconThickness = IsSelected ? 2.3 : 1.83;

        if (IconContent is Canvas canvas)
            foreach (var path in canvas.Children.OfType<PathShape>())
            {
                path.Stroke = iconStroke;
                path.StrokeThickness = iconThickness;
            }
    }
}
