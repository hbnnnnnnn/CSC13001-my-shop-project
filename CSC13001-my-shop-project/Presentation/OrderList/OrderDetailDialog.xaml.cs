using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace CSC13001_my_shop_project.Presentation.OrderList;

public sealed partial class OrderDetailDialog : ContentDialog
{
    public OrderDetailDialog()
    {
        this.InitializeComponent();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        this.Hide();
    }
}

public class OrderDetailViewModel
{
    public OrderItem Order { get; }
    public string DialogTitle => $"DETAILS OF ORDER {Order.Id}";

    public OrderDetailViewModel(OrderItem order)
    {
        Order = order;
    }
}
