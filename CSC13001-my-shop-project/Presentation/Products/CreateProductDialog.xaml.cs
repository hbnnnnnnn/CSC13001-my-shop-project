using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows.System;

namespace CSC13001_my_shop_project.Presentation.Products;

public sealed partial class CreateProductDialog : UserControl
{
    public CreateProductDialog()
    {
        this.InitializeComponent();
    }

    private void TagEntryBox_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key != VirtualKey.Enter || DataContext is not CreateProductViewModel vm)
            return;
        vm.AddTagCommand.Execute(null);
        e.Handled = true;
    }

    private void UploadZone_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
    {
        if (DataContext is CreateProductViewModel vm)
            vm.AddImagesFromPickerCommand.Execute(null);
    }
}
