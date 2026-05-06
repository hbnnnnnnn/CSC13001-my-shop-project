namespace CSC13001_my_shop_project.Services;

internal static class ReportDocuments
{
    internal const string CategorySalesReport = """
        query CategorySalesReport($period: String!, $startDate: String, $endDate: String) {
          categorySalesReport(period: $period, startDate: $startDate, endDate: $endDate) {
            period
            date
            totalQuantity
            totalRevenue
            totalCost
            totalProfit
            categories {
              category_id
              category_name
              quantity
              revenue
              totalCost
              totalProfit
            }
          }
        }
        """;

    internal const string ProductSalesReport = """
        query ProductSalesReport($period: String!, $startDate: String, $endDate: String, $categoryId: ID) {
          productSalesReport(period: $period, startDate: $startDate, endDate: $endDate, categoryId: $categoryId) {
            period
            date
            totalQuantity
            totalRevenue
            totalCost
            totalProfit
            products {
              product_id
              sku
              name
              quantity
              revenue
              totalCost
              totalProfit
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
