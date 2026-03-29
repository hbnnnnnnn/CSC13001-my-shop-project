using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace CSC13001_my_shop_project.Presentation.Products;

public sealed class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var flag = value is true;
        if (parameter is string s && s.Equals("invert", StringComparison.OrdinalIgnoreCase))
            flag = !flag;
        return flag ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotSupportedException();
}

public sealed class ViewModeSegmentIconBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var isGridView = value is true;
        var segment = parameter as string;
        var isGridSegment = string.Equals(segment, "grid", StringComparison.OrdinalIgnoreCase);
        var active = isGridView == isGridSegment;
        return active
            ? new SolidColorBrush(Color.FromArgb(0xFF, 0xD4, 0xA0, 0x56))
            : new SolidColorBrush(Color.FromArgb(0xFF, 0x9E, 0x9E, 0x9E));
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotSupportedException();
}

public sealed class BoolToToggleChromeBackgroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var flag = value is true;
        if (parameter is string s && s.Equals("invert", StringComparison.OrdinalIgnoreCase))
            flag = !flag;
        return flag
            ? new SolidColorBrush(Color.FromArgb(0x40, 0xF3, 0xB5, 0x5C))
            : new SolidColorBrush(Color.FromArgb(0x01, 0xFF, 0xFF, 0xFF));
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotSupportedException();
}

public sealed class PaginationNumberBackgroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (
            value is true
            && Application.Current.Resources.ContainsKey("ShellAccentBrush")
            && Application.Current.Resources["ShellAccentBrush"] is Brush a
        )
            return a;
        return new SolidColorBrush(Color.FromArgb(0x01, 0xFF, 0xFF, 0xFF));
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotSupportedException();
}

public sealed class PaginationNumberForegroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is true)
            return new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));
        if (
            Application.Current.Resources.ContainsKey("ShellTextStrongBrush")
            && Application.Current.Resources["ShellTextStrongBrush"] is Brush b
        )
            return b;
        return new SolidColorBrush(Color.FromArgb(0xFF, 0x2C, 0x21, 0x18));
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotSupportedException();
}
