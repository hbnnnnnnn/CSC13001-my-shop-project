using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace CSC13001_my_shop_project.Presentation.Products.Converters;

public sealed class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) =>
        value is true ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotSupportedException();
}
