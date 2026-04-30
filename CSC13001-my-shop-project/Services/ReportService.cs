using System.Text.Json.Serialization;
using CSC13001_my_shop_project.Models;

namespace CSC13001_my_shop_project.Services;

public sealed class ReportService(GraphQlClient gql) : IReportService
{
    public async Task<ReportBundleDto> LoadReportsAsync(
        string period,
        string? startDate,
        string? endDate,
        int topSellingLimit = 12,
        CancellationToken cancellationToken = default)
    {
        var vars = new { period, startDate, endDate };
        var topVars = new { limit = topSellingLimit, startDate, endDate };
        var overviewVars = new { startDate, endDate };

        var productTask = gql.ExecuteAsync<ProductSalesReportRoot>(
            ReportDocuments.ProductSalesReport,
            vars,
            cancellationToken);
        var revenueTask = gql.ExecuteAsync<RevenueReportRoot>(
            ReportDocuments.RevenueReport,
            vars,
            cancellationToken);
        var topTask = gql.ExecuteAsync<TopSellingReportRoot>(
            ReportDocuments.TopSellingProductsReport,
            topVars,
            cancellationToken);
        var overviewTask = gql.ExecuteAsync<SalesOverviewReportRoot>(
            ReportDocuments.SalesOverview,
            overviewVars,
            cancellationToken);

        await Task.WhenAll(productTask, revenueTask, topTask, overviewTask).ConfigureAwait(false);

        var productRoot = await productTask.ConfigureAwait(false);
        var revenueRoot = await revenueTask.ConfigureAwait(false);
        var topRoot = await topTask.ConfigureAwait(false);
        var overviewRoot = await overviewTask.ConfigureAwait(false);

        return new ReportBundleDto(
            productRoot?.ProductSalesReport ?? [],
            revenueRoot?.RevenueReport ?? [],
            topRoot?.TopSellingProductsReport ?? [],
            overviewRoot?.SalesOverview);
    }

    private sealed class ProductSalesReportRoot
    {
        [JsonPropertyName("productSalesReport")]
        public List<ProductSalesPeriodDto>? ProductSalesReport { get; set; }
    }

    private sealed class RevenueReportRoot
    {
        [JsonPropertyName("revenueReport")]
        public List<RevenuePeriodDto>? RevenueReport { get; set; }
    }

    private sealed class TopSellingReportRoot
    {
        [JsonPropertyName("topSellingProductsReport")]
        public List<TopSellingProductDto>? TopSellingProductsReport { get; set; }
    }

    private sealed class SalesOverviewReportRoot
    {
        [JsonPropertyName("salesOverview")]
        public SalesOverviewDto? SalesOverview { get; set; }
    }
}
