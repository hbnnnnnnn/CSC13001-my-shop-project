using System.Collections.ObjectModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace CSC13001_my_shop_project.Presentation.Products.Controls;

public sealed partial class CurrencyInputControl : UserControl
{
    public static readonly DependencyProperty AmountProperty = DependencyProperty.Register(
        nameof(Amount),
        typeof(string),
        typeof(CurrencyInputControl),
        new PropertyMetadata("")
    );

    public static readonly DependencyProperty CurrencyProperty = DependencyProperty.Register(
        nameof(Currency),
        typeof(string),
        typeof(CurrencyInputControl),
        new PropertyMetadata("USD")
    );

    public CurrencyInputControl()
    {
        this.InitializeComponent();
        CurrencyChoices = new ObservableCollection<string>(["USD", "VND", "EUR"]);
    }

    public ObservableCollection<string> CurrencyChoices { get; }

    public string Amount
    {
        get => (string)GetValue(AmountProperty);
        set => SetValue(AmountProperty, value);
    }

    public string Currency
    {
        get => (string)GetValue(CurrencyProperty);
        set => SetValue(CurrencyProperty, value);
    }
}
