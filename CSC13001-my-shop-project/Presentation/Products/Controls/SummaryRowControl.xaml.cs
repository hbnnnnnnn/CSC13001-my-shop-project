using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace CSC13001_my_shop_project.Presentation.Products.Controls;

public sealed partial class SummaryRowControl : UserControl
{
    public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
        nameof(Label),
        typeof(string),
        typeof(SummaryRowControl),
        new PropertyMetadata("")
    );

    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value),
        typeof(string),
        typeof(SummaryRowControl),
        new PropertyMetadata("")
    );

    public static readonly DependencyProperty ShowBottomDividerProperty = DependencyProperty.Register(
        nameof(ShowBottomDivider),
        typeof(bool),
        typeof(SummaryRowControl),
        new PropertyMetadata(true)
    );

    public SummaryRowControl()
    {
        this.InitializeComponent();
    }

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public string Value
    {
        get => (string)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public bool ShowBottomDivider
    {
        get => (bool)GetValue(ShowBottomDividerProperty);
        set => SetValue(ShowBottomDividerProperty, value);
    }
}
