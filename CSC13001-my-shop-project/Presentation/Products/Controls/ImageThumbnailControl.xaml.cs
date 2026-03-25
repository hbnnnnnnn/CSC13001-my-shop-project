using System.ComponentModel;
using CSC13001_my_shop_project.Models;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;

namespace CSC13001_my_shop_project.Presentation.Products.Controls;

public sealed partial class ImageThumbnailControl : UserControl
{
    public static readonly DependencyProperty ItemProperty = DependencyProperty.Register(
        nameof(Item),
        typeof(ImageItem),
        typeof(ImageThumbnailControl),
        new PropertyMetadata(null, OnItemChanged)
    );

    public ImageItem? Item
    {
        get => (ImageItem?)GetValue(ItemProperty);
        set => SetValue(ItemProperty, value);
    }

    public ImageThumbnailControl()
    {
        this.InitializeComponent();
    }

    private static void OnItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var c = (ImageThumbnailControl)d;
        if (e.OldValue is ImageItem oldItem)
            oldItem.PropertyChanged -= c.OnItemPropertyChanged;
        if (e.NewValue is ImageItem newItem)
        {
            newItem.PropertyChanged += c.OnItemPropertyChanged;
            c.Refresh();
        }
        else
            c.ClearThumb();
    }

    private void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(ImageItem.IsCover) or null)
            Refresh();
    }

    private void ClearThumb()
    {
        ThumbImage.Source = null;
        CoverBadge.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
    }

    private void Refresh()
    {
        if (Item is null)
        {
            ClearThumb();
            return;
        }

        CoverBadge.Visibility = Item.IsCover
            ? Microsoft.UI.Xaml.Visibility.Visible
            : Microsoft.UI.Xaml.Visibility.Collapsed;

        try
        {
            if (!string.IsNullOrEmpty(Item.RemoteUrl))
                ThumbImage.Source = new BitmapImage(new Uri(Item.RemoteUrl));
            else if (!string.IsNullOrEmpty(Item.LocalPath))
                ThumbImage.Source = new BitmapImage(new Uri(Item.LocalPath));
            else
                ThumbImage.Source = null;
        }
        catch
        {
            ThumbImage.Source = null;
        }
    }
}
