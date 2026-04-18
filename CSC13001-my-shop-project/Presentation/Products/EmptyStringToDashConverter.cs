using Microsoft.UI.Xaml.Data;

namespace CSC13001_my_shop_project.Presentation.Products;

public sealed class EmptyStringToDashConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, string language)
    {
        if (value is null)
            return "—";
        var s = value.ToString();
        return string.IsNullOrWhiteSpace(s) ? "—" : s!;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language) =>
        throw new NotSupportedException();
}
