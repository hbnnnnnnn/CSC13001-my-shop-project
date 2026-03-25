using Microsoft.UI.Xaml.Media;

namespace CSC13001_my_shop_project.Models;

public sealed class OrderModel
{
    public OrderModel(
        string orderId,
        string customer,
        string date,
        int quantity,
        string total,
        string status,
        SolidColorBrush rowBackground
    )
    {
        OrderId = orderId;
        Customer = customer;
        Date = date;
        Quantity = quantity;
        Total = total;
        Status = status;
        RowBackground = rowBackground;
    }

    public string OrderId { get; }
    public string Customer { get; }
    public string Date { get; }
    public int Quantity { get; }
    public string Total { get; }
    public string Status { get; }
    public SolidColorBrush RowBackground { get; }
}
