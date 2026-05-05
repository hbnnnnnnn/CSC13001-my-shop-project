using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace CSC13001_my_shop_project.Presentation.Products;

public sealed partial class DeleteProductSuccessDialog : ContentDialog
{
    public DeleteProductSuccessDialog()
    {
        this.InitializeComponent();
    }

    public void SetProductName(string name)
    {
        SuccessMessage.Text = $"Product {name} has been successfully deleted.";
    }

    private void OkBtn_Click(object sender, RoutedEventArgs e)
    {
        Hide();
    }
}
