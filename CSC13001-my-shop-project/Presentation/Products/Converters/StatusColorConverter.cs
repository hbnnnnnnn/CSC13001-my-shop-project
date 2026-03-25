using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace CSC13001_my_shop_project.Presentation.Products.Converters;

/// <summary>Maps order status strings to background or foreground brushes (ConverterParameter: Background | Foreground).</summary>
public sealed class StatusColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var key = parameter as string ?? "Background";
        var s = value as string ?? "";
        var bg = key.Equals("Foreground", StringComparison.OrdinalIgnoreCase) ? false : true;

        return s switch
        {
            "Delivered" => bg
                ? new SolidColorBrush(Color.FromArgb(0xFF, 0xD1, 0xFA, 0xE5))
                : new SolidColorBrush(Color.FromArgb(0xFF, 0x16, 0x5F, 0x34)),
            "Shipped" => bg
                ? new SolidColorBrush(Color.FromArgb(0xFF, 0xDB, 0xEA, 0xFE))
                : new SolidColorBrush(Color.FromArgb(0xFF, 0x1E, 0x40, 0xAF)),
            "Processing" => bg
                ? new SolidColorBrush(Color.FromArgb(0xFF, 0xFE, 0xF3, 0xC7))
                : new SolidColorBrush(Color.FromArgb(0xFF, 0xB4, 0x53, 0x09)),
            _ => bg
                ? new SolidColorBrush(Color.FromArgb(0xFF, 0xF3, 0xF4, 0xF6))
                : new SolidColorBrush(Color.FromArgb(0xFF, 0x6B, 0x72, 0x80)),
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotSupportedException();
}
