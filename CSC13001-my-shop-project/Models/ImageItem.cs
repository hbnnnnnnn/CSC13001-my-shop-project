using CommunityToolkit.Mvvm.ComponentModel;

namespace CSC13001_my_shop_project.Models;

public sealed class ImageItem : ObservableObject
{
    public string? LocalPath { get; set; }

    public string? RemoteUrl { get; set; }

    public string FileName { get; set; } = "";

    private bool _isCover;

    public bool IsCover
    {
        get => _isCover;
        set => SetProperty(ref _isCover, value);
    }
}
