using System.ComponentModel;
using Microsoft.UI.Xaml;
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

    public ImageThumbnailControl()
    {
        this.InitializeComponent();
    }

    public ImageItem? Item
    {
        get => (ImageItem?)GetValue(ItemProperty);
        set => SetValue(ItemProperty, value);
    }

    static void OnItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var c = (ImageThumbnailControl)d;
        if (e.OldValue is ImageItem old)
            old.PropertyChanged -= c.OnImageItemPropertyChanged;
        if (e.NewValue is ImageItem ni)
            ni.PropertyChanged += c.OnImageItemPropertyChanged;
        c.RefreshImage();
    }

    void OnImageItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (
            e.PropertyName
                is nameof(ImageItem.RemoteUrl)
                or nameof(ImageItem.LocalPath)
                or nameof(ImageItem.IsCover)
        )
            RefreshImage();
    }

    void RefreshImage()
    {
        var path = Item?.RemoteUrl ?? Item?.LocalPath;
        if (string.IsNullOrWhiteSpace(path))
        {
            PreviewImage.Source = null;
            return;
        }

        try
        {
            var uri =
                path.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("ms-appx:", StringComparison.OrdinalIgnoreCase)
                    ? new Uri(path)
                    : new Uri(path, UriKind.Absolute);
            PreviewImage.Source = new BitmapImage(uri);
        }
        catch
        {
            PreviewImage.Source = null;
        }
    }
}
