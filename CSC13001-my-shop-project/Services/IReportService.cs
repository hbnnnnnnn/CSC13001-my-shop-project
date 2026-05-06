using CSC13001_my_shop_project.Models;

namespace CSC13001_my_shop_project.Services;

public interface IReportService
{
    Task<ReportBundleDto> LoadReportsAsync(
        string period,
        string? startDate,
        string? endDate,
        int topSellingLimit = 12,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProductSalesPeriodDto>> LoadProductSalesAsync(
        string period,
        string? startDate,
        string? endDate,
        string? categoryId,
        CancellationToken cancellationToken = default);
}

public sealed record ReportBundleDto(
    IReadOnlyList<CategorySalesPeriodDto> CategorySales,
    IReadOnlyList<RevenuePeriodDto> Revenue,
    IReadOnlyList<TopSellingProductDto> TopSelling,
    SalesOverviewDto? SalesOverview);
