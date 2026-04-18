namespace CSC13001_my_shop_project.Services;

public interface IImageUploadService
{
    /// <summary>Uploads image bytes to <c>POST /api/upload</c>; returns Cloudinary URL or null.</summary>
    Task<string?> UploadImageAsync(Stream imageStream, string fileName);
}
