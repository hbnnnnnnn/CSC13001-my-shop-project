namespace CSC13001_my_shop_project.Services;

internal static class ReportDocuments
{
    internal const string ProductSalesReport = """
        query ProductSalesReport($period: String!, $startDate: String, $endDate: String) {
          productSalesReport(period: $period, startDate: $startDate, endDate: $endDate) {
            period
            date
            totalQuantity
            totalRevenue
            products {
              product_id
              sku
              name
              quantity
              revenue
            }
          }
        }
        """;

    internal const string RevenueReport = """
        query RevenueReport($period: String!, $startDate: String, $endDate: String) {
          revenueReport(period: $period, startDate: $startDate, endDate: $endDate) {
            period
            date
            totalOrders
            totalRevenue
            totalItemsSold
            avgOrderValue
          }
        }
        """;

    internal const string TopSellingProductsReport = """
        query TopSellingProductsReport($limit: Int!, $startDate: String, $endDate: String) {
          topSellingProductsReport(limit: $limit, startDate: $startDate, endDate: $endDate) {
            product_id
            sku
            name
            price
            totalQuantity
            totalRevenue
            timesSold
          }
        }
        """;

    internal const string SalesOverview = """
        query SalesOverview($startDate: String, $endDate: String) {
          salesOverview(startDate: $startDate, endDate: $endDate) {
            totalOrders
            totalRevenue
            totalItemsSold
            uniqueCustomers
            avgOrderValue
            maxOrderValue
            minOrderValue
          }
        }
        """;
}
