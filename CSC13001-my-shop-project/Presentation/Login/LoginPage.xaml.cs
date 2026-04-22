using CommunityToolkit.Mvvm.Messaging;
using CSC13001_my_shop_project.Presentation.Dashboard;
using CSC13001_my_shop_project.Presentation.Helpers;
using Microsoft.UI;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;

namespace CSC13001_my_shop_project.Presentation.Login;

public sealed partial class LoginPage : Page
{
    private Brush? _signInDefaultBackground;
    private Brush? _signInHoverBackground;
    private Brush? _signInDefaultForeground;
    private readonly Brush _signInHoverForeground = new SolidColorBrush(Colors.White);
    private Brush? _configurationButtonDefaultBackground;
    private Brush? _configurationButtonDefaultBorder;
    private Brush? _configurationButtonDefaultIcon;
    private Brush? _configurationButtonHoverBackground;
    private Brush? _configurationButtonHoverIcon;

    public LoginPage()
    {
        this.InitializeComponent();
        _signInDefaultBackground = SignInButton.Background;
        _signInHoverBackground = ResolveBrush("AuthPrimaryButtonHoverBrush");
        _signInDefaultForeground = SignInButton.Foreground;
        _configurationButtonDefaultBackground = ConfigurationButton.Background;
        _configurationButtonDefaultBorder = ConfigurationButton.BorderBrush;
        _configurationButtonDefaultIcon = ConfigurationButtonOuterIcon.Stroke;
        _configurationButtonHoverBackground = ResolveBrush(
            "AuthSettingsButtonHoverBackgroundBrush"
        );
        _configurationButtonHoverIcon = ResolveBrush("AuthSettingsButtonHoverIconBrush");
        SizeChanged += OnSizeChanged;
        Loaded += (_, _) => WeakReferenceMessenger.Default.Send(new ChromeVisibilityMessage(false));
    }

    private void OnSignInPointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (sender is Button button)
        {
            button.Background =
                _signInHoverBackground ?? ResolveBrush("AuthPrimaryButtonHoverBrush");
            button.Foreground = _signInHoverForeground;
        }
    }

    private void OnSignInPointerExited(object sender, PointerRoutedEventArgs e)
    {
        if (sender is Button button)
        {
            button.Background = _signInDefaultBackground ?? ResolveBrush("AuthPrimaryButtonBrush");
            button.Foreground = _signInDefaultForeground ?? _signInHoverForeground;
        }
    }

    private void OnConfigurationButtonPointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (sender is not Button button)
        {
            return;
        }

        button.Background = _configurationButtonHoverBackground ?? ResolveBrush("BrandColor");
        button.BorderBrush = _configurationButtonHoverBackground ?? ResolveBrush("BrandColor");
        var hoverIconBrush = _configurationButtonHoverIcon ?? new SolidColorBrush(Colors.White);
        ConfigurationButtonOuterIcon.Stroke = hoverIconBrush;
        ConfigurationButtonInnerIcon.Stroke = hoverIconBrush;
    }

    private void OnConfigurationButtonPointerExited(object sender, PointerRoutedEventArgs e)
    {
        if (sender is not Button button)
        {
            return;
        }

        button.Background =
            _configurationButtonDefaultBackground
            ?? ResolveBrush("AuthSettingsButtonBackgroundBrush");
        button.BorderBrush = _configurationButtonDefaultBorder ?? ResolveBrush("BrandColor");
        var defaultIconBrush = _configurationButtonDefaultIcon ?? ResolveBrush("BrandColor");
        ConfigurationButtonOuterIcon.Stroke = defaultIconBrush;
        ConfigurationButtonInnerIcon.Stroke = defaultIconBrush;
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e) =>
        HeroAdaptiveHelper.Apply(
            e.NewSize.Width,
            920,
            HeroPanel,
            LeftColumn,
            new GridLength(1.08, GridUnitType.Star)
        );

    private void OnClearTextClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button { CommandParameter: TextBox textBox })
        {
            textBox.Text = string.Empty;
            textBox.Focus(FocusState.Programmatic);
        }
    }

    private void OnTogglePasswordVisibilityClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is not LoginViewModel viewModel)
        {
            return;
        }

        viewModel.IsPasswordVisible = !viewModel.IsPasswordVisible;

        if (viewModel.IsPasswordVisible)
        {
            if (FindName("PasswordVisibleInput") is Control passwordVisibleInput)
            {
                passwordVisibleInput.Focus(FocusState.Programmatic);
            }
        }
        else
        {
            if (FindName("PasswordHiddenInput") is Control passwordHiddenInput)
            {
                passwordHiddenInput.Focus(FocusState.Programmatic);
            }
        }
    }

    private void OnFieldElementGotFocus(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: string fieldKey })
        {
            SetFieldChromeState(fieldKey, true);
        }
    }

    private void OnFieldElementLostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: string fieldKey })
        {
            DispatcherQueue.TryEnqueue(() =>
                SetFieldChromeState(fieldKey, IsFieldFocused(fieldKey))
            );
        }
    }

    private bool IsFieldFocused(string fieldKey)
    {
        if (XamlRoot is null || FindName($"{fieldKey}FieldChrome") is not FrameworkElement root)
        {
            return false;
        }

        var focusedElement = FocusManager.GetFocusedElement(XamlRoot) as DependencyObject;
        while (focusedElement is not null)
        {
            if (ReferenceEquals(focusedElement, root))
            {
                return true;
            }

            focusedElement = VisualTreeHelper.GetParent(focusedElement);
        }

        return false;
    }

    private void SetFieldChromeState(string fieldKey, bool isFocused)
    {
        if (FindName($"{fieldKey}FieldGlow") is Border glow)
        {
            glow.Opacity = isFocused ? 0.12 : 0;
        }

        if (FindName($"{fieldKey}FieldRing") is Border ring)
        {
            ring.Opacity = isFocused ? 0.45 : 0;
        }

        if (FindName($"{fieldKey}FieldSurface") is Border surface)
        {
            surface.Background = ResolveBrush(
                isFocused ? "AuthInputFocusBackgroundBrush" : "AuthInputBackgroundBrush"
            );
            surface.BorderBrush = ResolveBrush(
                isFocused ? "AuthInputFocusRingBrush" : "AuthInputBorderBrush"
            );
        }
    }

    private Brush ResolveBrush(string key)
    {
        if (Resources.TryGetValue(key, out var localResource) && localResource is Brush localBrush)
        {
            return localBrush;
        }

        if (
            Application.Current.Resources.TryGetValue(key, out var appResource)
            && appResource is Brush appBrush
        )
        {
            return appBrush;
        }

        return new SolidColorBrush(Colors.Transparent);
    }
}
