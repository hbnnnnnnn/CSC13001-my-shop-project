using System.IO;

namespace CSC13001_my_shop_project.Services;

public interface IImportService
{
    Task<ProductImportResult> ImportProductsAsync(Stream fileStream, string fileName, CancellationToken ct = default);
}

public sealed record ProductImportError(int Row, string? Sku, string? Reason);

public sealed record ProductImportSummary(int TotalRows, int Upserted, int Errors);

public sealed record ProductImportResult(
    bool Success,
    string? Message,
    ProductImportSummary? Summary,
    IReadOnlyList<ProductImportError> Errors);
