using CommunityToolkit.Mvvm.Messaging;
using CSC13001_my_shop_project.Presentation.Dashboard;
using CSC13001_my_shop_project.Presentation.Helpers;

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
    HeroAdaptiveHelper.Apply(e.NewSize.Width, 920, HeroPanel, LeftColumn, new GridLength(1.08, GridUnitType.Star));
}
