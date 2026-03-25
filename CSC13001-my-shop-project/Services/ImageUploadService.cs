using System.Net.Http.Json;
using System.Text.Json.Serialization;
using CSC13001_my_shop_project.Models;
using Microsoft.Extensions.Options;

namespace CSC13001_my_shop_project.Services;

public sealed class ImageUploadService(HttpClient http, IOptions<AppConfig> appConfig) : IImageUploadService
{
    public async Task<string?> UploadImageAsync(Stream fileStream, string fileName, CancellationToken ct = default)
    {
        var ep = appConfig.Value.GraphQlEndpoint ?? "http://localhost:4000/graphql";
        var u = new Uri(ep);
        var root = $"{u.Scheme}://{u.Authority}";

        using var content = new MultipartFormDataContent();
        using var streamContent = new StreamContent(fileStream);
        streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
        content.Add(streamContent, "image", fileName);

        using var resp = await http.PostAsync($"{root}/api/upload/", content, ct).ConfigureAwait(false);
        if (!resp.IsSuccessStatusCode)
            return null;

        var body = await resp.Content.ReadFromJsonAsync<UploadResponse>(ct).ConfigureAwait(false);
        return body?.ImageUrl;
    }

    private sealed record UploadResponse([property: JsonPropertyName("imageUrl")] string? ImageUrl);
}
