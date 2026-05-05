using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;

namespace CSC13001_my_shop_project.Presentation.Products;

public sealed partial class DeleteProductDialog : ContentDialog
{
    public bool IsConfirmed { get; private set; }

    public string ProductName { get; set; } = string.Empty;

    public DeleteProductDialog()
    {
        this.InitializeComponent();
    }

    public void SetProductName(string name)
    {
        ProductName = name;

        var message = new Span();

        var run1 = new Run { Text = "Are you sure about deleting product " };
        run1.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(0x99, 0x2C, 0x21, 0x18));

        var runName = new Run { Text = name, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold };
        runName.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(0xFF, 0x2C, 0x21, 0x18));

        var run2 = new Run { Text = "? This action cannot be undone." };
        run2.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(0x99, 0x2C, 0x21, 0x18));

        ConfirmMessage.Inlines.Clear();
        ConfirmMessage.Inlines.Add(run1);
        ConfirmMessage.Inlines.Add(runName);
        ConfirmMessage.Inlines.Add(run2);
    }

    private void CancelBtn_Click(object sender, RoutedEventArgs e)
    {
        IsConfirmed = false;
        Hide();
    }

    private void DeleteBtn_Click(object sender, RoutedEventArgs e)
    {
        IsConfirmed = true;
        Hide();
    }
}
