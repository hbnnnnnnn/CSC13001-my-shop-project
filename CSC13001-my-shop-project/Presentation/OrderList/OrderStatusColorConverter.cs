namespace CSC13001_my_shop_project.Presentation.OrderList;

using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;

public class OrderStatusColorConverter : IValueConverter
{
    public Brush? CreatedColor { get; set; }
    public Brush? ProcessingColor { get; set; }
    public Brush? ShippedColor { get; set; }
    public Brush? DeliveredColor { get; set; }
    public Brush? PendingColor { get; set; }
    public Brush? CancelledColor { get; set; }
    public Brush? DefaultColor { get; set; }

    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string status)
        {
            return status switch
            {
                "Created" => CreatedColor,
                "Processing" => ProcessingColor,
                "Shipped" => ShippedColor,
                "Delivered" => DeliveredColor,
                "Pending" => PendingColor,
                "Cancelled" => CancelledColor,
                _ => DefaultColor
            };
        }
        return DefaultColor;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}
