using Microsoft.UI.Xaml.Data;

namespace CSC13001_my_shop_project.Presentation.Products.Converters;

/// <summary>Formats a 0–5 rating as a simple star string (e.g. ★★★★½).</summary>
public sealed class StarRatingConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var r = value switch
        {
            double d => d,
            float f => f,
            int i => (double)i,
            _ => 0d,
        };
        r = Math.Clamp(r, 0, 5);
        var full = (int)Math.Floor(r);
        var half = r - full >= 0.5 && full < 5;
        var empty = 5 - full - (half ? 1 : 0);
        return new string('★', full) + (half ? "½" : "") + new string('☆', Math.Max(0, empty));
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotSupportedException();
}
