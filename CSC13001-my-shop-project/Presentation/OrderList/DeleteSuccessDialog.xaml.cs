using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace CSC13001_my_shop_project.Presentation.OrderList;

public sealed partial class DeleteSuccessDialog : ContentDialog
{
    public DeleteSuccessDialog()
    {
        this.InitializeComponent();
    }

    public void SetOrderId(string orderId)
    {
        SuccessMessage.Text = $"Order {orderId} has been successfully deleted.";
    }

    private void OkBtn_Click(object sender, RoutedEventArgs e)
    {
        Hide();
    }
}
