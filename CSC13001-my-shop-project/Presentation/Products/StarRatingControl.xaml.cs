using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace CSC13001_my_shop_project.Presentation.Products;

public sealed partial class StarRatingControl : UserControl
{
    public static readonly DependencyProperty RatingProperty = DependencyProperty.Register(
        nameof(Rating),
        typeof(double),
        typeof(StarRatingControl),
        new PropertyMetadata(0d, OnRatingChanged)
    );

    private static readonly SolidColorBrush Gold = new(Color.FromArgb(0xFF, 0xF5, 0x9E, 0x0B));
    private static readonly SolidColorBrush Muted = new(Color.FromArgb(0xFF, 0xD1, 0xD5, 0xDB));

    public StarRatingControl()
    {
        InitializeComponent();
        ApplyStars(Rating);
    }

    public double Rating
    {
        get => (double)GetValue(RatingProperty);
        set => SetValue(RatingProperty, value);
    }

    private static void OnRatingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is StarRatingControl c)
            c.ApplyStars((double)e.NewValue);
    }

    private void ApplyStars(double r)
    {
        r = Math.Clamp(r, 0, 5);
        var blocks = new[] { Star0, Star1, Star2, Star3, Star4 };
        for (var i = 0; i < 5; i++)
        {
            var segment = r - i;
            if (segment >= 1)
            {
                blocks[i].Text = "★";
                blocks[i].Foreground = Gold;
                blocks[i].Opacity = 1;
            }
            else if (segment >= 0.5)
            {
                blocks[i].Text = "★";
                blocks[i].Foreground = Gold;
                blocks[i].Opacity = 0.55;
            }
            else
            {
                blocks[i].Text = "☆";
                blocks[i].Foreground = Muted;
                blocks[i].Opacity = 1;
            }
        }
    }
}
