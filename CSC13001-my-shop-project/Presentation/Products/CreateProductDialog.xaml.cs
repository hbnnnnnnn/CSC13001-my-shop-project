using CSC13001_my_shop_project.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
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

    private void CategoryPickerHost_Tapped(object sender, TappedRoutedEventArgs e) => ShowCategoryFlyout(sender);

    private void CategoryPickerHost_PointerPressed(object sender, PointerRoutedEventArgs e) => ShowCategoryFlyout(sender);

    private void ShowCategoryFlyout(object sender)
    {
        if (sender is FrameworkElement fe)
            FlyoutBase.ShowAttachedFlyout(fe);
    }

    private void CategoryPickItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn || btn.DataContext is not CategoryDto picked || DataContext is not CreateProductViewModel vm)
            return;
        vm.SelectedCategoryItem = picked;
        FlyoutBase.GetAttachedFlyout(CategoryPickerHost)?.Hide();
    }
}
