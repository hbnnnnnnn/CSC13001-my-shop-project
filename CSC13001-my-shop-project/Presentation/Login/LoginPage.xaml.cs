using CommunityToolkit.Mvvm.Messaging;
using CSC13001_my_shop_project.Presentation.Dashboard;
using CSC13001_my_shop_project.Presentation.Helpers;
using Microsoft.UI;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;

namespace CSC13001_my_shop_project.Presentation.Login;

public sealed partial class LoginPage : Page
{
    public LoginPage()
    {
        this.InitializeComponent();
        SizeChanged += OnSizeChanged;
        Loaded += (_, _) => WeakReferenceMessenger.Default.Send(new ChromeVisibilityMessage(false));
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
