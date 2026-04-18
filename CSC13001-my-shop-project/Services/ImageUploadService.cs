using System.Net.Http.Json;
using System.Text.Json;

namespace CSC13001_my_shop_project.Services;

public sealed class ImageUploadService : IImageUploadService
{
    private readonly HttpClient _client;

    public ImageUploadService(IHttpClientFactory httpClientFactory)
    {
        _client = httpClientFactory.CreateClient("BackendApi");
    }

    public async Task<string?> UploadImageAsync(Stream imageStream, string fileName)
    {
        using var content = new MultipartFormDataContent();
        using var streamContent = new StreamContent(imageStream);
        content.Add(streamContent, "image", fileName);

        var response = await _client.PostAsync("/api/upload", content);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        if (json.TryGetProperty("imageUrl", out var urlEl))
            return urlEl.GetString();

        return null;
    }
}
