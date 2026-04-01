using System.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace CSC13001_my_shop_project.Presentation.Layout;

public sealed partial class AvatarBadgeControl : UserControl
{
    public AvatarBadgeControl()
    {
        this.InitializeComponent();
        UpdateInitials();
    }

    public static readonly DependencyProperty DisplayNameProperty = DependencyProperty.Register(
        nameof(DisplayName),
        typeof(string),
        typeof(AvatarBadgeControl),
        new PropertyMetadata(string.Empty, OnAvatarInputChanged)
    );

    public static readonly DependencyProperty FallbackInitialsProperty =
        DependencyProperty.Register(
            nameof(FallbackInitials),
            typeof(string),
            typeof(AvatarBadgeControl),
            new PropertyMetadata("AD", OnAvatarInputChanged)
        );

    public static readonly DependencyProperty AvatarSizeProperty = DependencyProperty.Register(
        nameof(AvatarSize),
        typeof(double),
        typeof(AvatarBadgeControl),
        new PropertyMetadata(32d)
    );

    public static readonly DependencyProperty InitialsFontSizeProperty =
        DependencyProperty.Register(
            nameof(InitialsFontSize),
            typeof(double),
            typeof(AvatarBadgeControl),
            new PropertyMetadata(12d)
        );

    public static readonly DependencyProperty InitialsProperty = DependencyProperty.Register(
        nameof(Initials),
        typeof(string),
        typeof(AvatarBadgeControl),
        new PropertyMetadata("AD")
    );

    public string DisplayName
    {
        get => (string)GetValue(DisplayNameProperty);
        set => SetValue(DisplayNameProperty, value);
    }

    public string FallbackInitials
    {
        get => (string)GetValue(FallbackInitialsProperty);
        set => SetValue(FallbackInitialsProperty, value);
    }

    public double AvatarSize
    {
        get => (double)GetValue(AvatarSizeProperty);
        set => SetValue(AvatarSizeProperty, value);
    }

    public double InitialsFontSize
    {
        get => (double)GetValue(InitialsFontSizeProperty);
        set => SetValue(InitialsFontSizeProperty, value);
    }

    public string Initials
    {
        get => (string)GetValue(InitialsProperty);
        private set => SetValue(InitialsProperty, value);
    }

    private static void OnAvatarInputChanged(
        DependencyObject dependencyObject,
        DependencyPropertyChangedEventArgs args
    )
    {
        if (dependencyObject is AvatarBadgeControl control)
        {
            control.UpdateInitials();
        }
    }

    private void UpdateInitials()
    {
        var computed = ComputeInitials(DisplayName);
        Initials = string.IsNullOrWhiteSpace(computed) ? FallbackInitials : computed;
    }

    private static string ComputeInitials(string? displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            return string.Empty;
        }

        var parts = displayName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
        {
            return string.Empty;
        }

        if (parts.Length == 1)
        {
            return TakeUpToTwoChars(parts[0]);
        }

        var initialsBuilder = new StringBuilder(2);
        initialsBuilder.Append(char.ToUpperInvariant(parts[0][0]));
        initialsBuilder.Append(char.ToUpperInvariant(parts[^1][0]));
        return initialsBuilder.ToString();
    }

    private static string TakeUpToTwoChars(string value)
    {
        var trimmed = value.Trim();
        if (trimmed.Length == 0)
        {
            return string.Empty;
        }

        if (trimmed.Length == 1)
        {
            return char.ToUpperInvariant(trimmed[0]).ToString();
        }

        var first = char.ToUpperInvariant(trimmed[0]);
        var second = char.ToUpperInvariant(trimmed[1]);
        return string.Concat(first, second);
    }
}
