using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace CSC13001_my_shop_project.Presentation.Products.Controls;

public sealed partial class SectionHeaderControl : UserControl
{
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
        nameof(Title),
        typeof(string),
        typeof(SectionHeaderControl),
        new PropertyMetadata("")
    );

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public SectionHeaderControl()
    {
        this.InitializeComponent();
    }
}
