using CommunityToolkit.Mvvm.ComponentModel;

namespace CSC13001_my_shop_project.Presentation.Products;

public partial class ImageItem : ObservableObject
{
    public string? LocalPath { get; set; }

    public string? FileName { get; set; }

    [ObservableProperty]
    private string? remoteUrl;

    [ObservableProperty]
    private bool isCover;
}
