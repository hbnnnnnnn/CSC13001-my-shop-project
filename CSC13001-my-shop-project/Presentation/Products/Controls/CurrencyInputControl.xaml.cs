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
        new PropertyMetadata("", OnAmountChanged)
    );

    public static readonly DependencyProperty CurrencyProperty = DependencyProperty.Register(
        nameof(Currency),
        typeof(string),
        typeof(CurrencyInputControl),
        new PropertyMetadata("USD", OnCurrencyChanged)
    );

    public ObservableCollection<string> CurrencyOptions { get; } = new(["USD", "EUR", "VND"]);

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

    public event EventHandler? AmountChanged;
    public event EventHandler? CurrencyChanged;

    public CurrencyInputControl()
    {
        this.InitializeComponent();
    }

    private static void OnAmountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
        ((CurrencyInputControl)d).AmountChanged?.Invoke(d, EventArgs.Empty);

    private static void OnCurrencyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
        ((CurrencyInputControl)d).CurrencyChanged?.Invoke(d, EventArgs.Empty);
}
