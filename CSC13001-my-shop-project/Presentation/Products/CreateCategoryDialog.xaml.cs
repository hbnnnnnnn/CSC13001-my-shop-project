using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace CSC13001_my_shop_project.Presentation.Products;

public sealed partial class CreateCategoryDialog : ContentDialog
{
    public CreateCategoryDialog()
    {
        this.InitializeComponent();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        Hide();
    }

    private async void Create_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not CreateCategoryViewModel vm)
            return;

        await vm.SubmitAsync();
        if (!vm.HasError)
            Hide();
    }
}
