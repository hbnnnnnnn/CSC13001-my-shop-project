using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;

namespace CSC13001_my_shop_project.Services;

public sealed class ImportService : IImportService
{
    private readonly HttpClient _client;

    public ImportService(IHttpClientFactory httpClientFactory)
    {
        _client = httpClientFactory.CreateClient("BackendApi");
    }

    public async Task<ProductImportResult> ImportProductsAsync(Stream fileStream, string fileName, CancellationToken ct = default)
    {
        using var content = new MultipartFormDataContent();
        using var streamContent = new StreamContent(fileStream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue(
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        content.Add(streamContent, "file", fileName);

        using var resp = await _client.PostAsync("/api/import/products", content, ct).ConfigureAwait(false);
        var body = await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

        string? message = null;
        ProductImportSummary? summary = null;
        var errors = new List<ProductImportError>();

        try
        {
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            if (root.TryGetProperty("message", out var msgEl))
                message = msgEl.GetString();

            if (root.TryGetProperty("summary", out var summaryEl) && summaryEl.ValueKind == JsonValueKind.Object)
            {
                summary = new ProductImportSummary(
                    summaryEl.TryGetProperty("total_rows", out var t) && t.TryGetInt32(out var tv) ? tv : 0,
                    summaryEl.TryGetProperty("upserted", out var u) && u.TryGetInt32(out var uv) ? uv : 0,
                    summaryEl.TryGetProperty("errors", out var e) && e.TryGetInt32(out var ev) ? ev : 0);
            }

            if (root.TryGetProperty("errors", out var errsEl) && errsEl.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in errsEl.EnumerateArray())
                {
                    var row = item.TryGetProperty("row", out var r) && r.TryGetInt32(out var rv) ? rv : 0;
                    var sku = item.TryGetProperty("sku", out var s) ? s.GetString() : null;
                    var reason = item.TryGetProperty("reason", out var rs) ? rs.GetString() : null;
                    errors.Add(new ProductImportError(row, sku, reason));
                }
            }
        }
        catch
        {
            // Fall through with raw body in message
            message ??= body;
        }

        return new ProductImportResult(resp.IsSuccessStatusCode, message, summary, errors);
    }
}
