using System;
using Microsoft.UI.Xaml.Data;

namespace CSC13001_my_shop_project.Presentation.Converters;

/// <summary>True (series visible) → full opacity; false (hidden) → dimmed.</summary>
public sealed class BooleanToLegendOpacityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var visible = value is bool b && b;
        return visible ? 1.0 : 0.4;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotSupportedException();
}
