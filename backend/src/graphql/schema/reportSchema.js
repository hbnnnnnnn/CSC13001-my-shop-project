const reportSchema = `#graphql
  type ProductSalesData {
    product_id: ID!
    sku: String!
    name: String!
    quantity: Int!
    revenue: Int!
  }

  type ProductSalesPeriod {
    period: String!
    date: String!
    products: [ProductSalesData!]!
    totalQuantity: Int!
    totalRevenue: Int!
  }

  type RevenuePeriod {
    period: String!
    date: String!
    totalOrders: Int!
    totalRevenue: Int!
    totalItemsSold: Int!
    avgOrderValue: Float!
  }

  type TopProduct {
    product_id: ID!
    sku: String!
    name: String!
    price: Int!
    totalQuantity: Int!
    totalRevenue: Int!
    timesSold: Int!
  }

  type SalesOverview {
    totalOrders: Int!
    totalRevenue: Int!
    totalItemsSold: Int!
    uniqueCustomers: Int!
    avgOrderValue: Float!
    maxOrderValue: Int!
    minOrderValue: Int!
  }

  type Query {
    productSalesReport(
      period: String
      startDate: String
      endDate: String
    ): [ProductSalesPeriod!]!

    revenueReport(
      period: String
      startDate: String
      endDate: String
    ): [RevenuePeriod!]!

    topSellingProducts(
      limit: Int
      startDate: String
      endDate: String
    ): [TopProduct!]!

    salesOverview(
      startDate: String
      endDate: String
    ): SalesOverview!
  }
`;

module.exports = reportSchema;
