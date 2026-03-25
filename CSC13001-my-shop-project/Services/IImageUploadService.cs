namespace CSC13001_my_shop_project.Services;

public interface IImageUploadService
{
    Task<string?> UploadImageAsync(Stream fileStream, string fileName, CancellationToken ct = default);
}
