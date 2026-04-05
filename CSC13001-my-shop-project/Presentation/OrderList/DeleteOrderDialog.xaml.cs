using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;

namespace CSC13001_my_shop_project.Presentation.OrderList;

public sealed partial class DeleteOrderDialog : ContentDialog
{
    public bool IsConfirmed { get; private set; }

    public string OrderId { get; set; } = string.Empty;

    public DeleteOrderDialog()
    {
        this.InitializeComponent();
    }

    public void SetOrderId(string orderId)
    {
        OrderId = orderId;

        // Build the rich text: "Are you sure about deleting Order #00132? This action cannot be undone."
        var message = new Microsoft.UI.Xaml.Documents.Span();

        var run1 = new Run { Text = "Are you sure about deleting Order " };
        run1.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(0x99, 0x2C, 0x21, 0x18));

        var runId = new Run { Text = orderId, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold };
        runId.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(0xFF, 0x2C, 0x21, 0x18));

        var run2 = new Run { Text = "? This action cannot be undone." };
        run2.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(0x99, 0x2C, 0x21, 0x18));

        ConfirmMessage.Inlines.Clear();
        ConfirmMessage.Inlines.Add(run1);
        ConfirmMessage.Inlines.Add(runId);
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
