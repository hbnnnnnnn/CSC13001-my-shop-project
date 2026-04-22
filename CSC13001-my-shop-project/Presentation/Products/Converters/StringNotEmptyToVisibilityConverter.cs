using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace CSC13001_my_shop_project.Presentation.Products.Converters;

public sealed class StringNotEmptyToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, string language)
    {
        var s = value as string;
        return string.IsNullOrWhiteSpace(s) ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language) =>
        throw new NotSupportedException();
}
