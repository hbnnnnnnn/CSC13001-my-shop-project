using CommunityToolkit.Mvvm.Messaging;
using CSC13001_my_shop_project.Presentation.Dashboard;
using CSC13001_my_shop_project.Presentation.Helpers;

namespace CSC13001_my_shop_project.Presentation.ServerConfiguration;

public sealed partial class ServerConfigurationPage : Page
{
    public ServerConfigurationPage()
    {
        this.InitializeComponent();
        SizeChanged += OnSizeChanged;
        Loaded += (_, _) => WeakReferenceMessenger.Default.Send(new ChromeVisibilityMessage(false));
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e) {
        var currentWidth = e.NewSize.Width;
        HeroAdaptiveHelper.Apply(currentWidth, 920, HeroPanel, LeftColumn, new GridLength(1.08, GridUnitType.Star));

        if (currentWidth < 1200)
        {
            FormStackPanel.Padding = new Thickness(48, 48);
        }
        else
        {
            FormStackPanel.Padding = new Thickness(0, 48);
        }


    }
}
