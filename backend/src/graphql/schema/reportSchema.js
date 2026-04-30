const reportSchema = `#graphql
  type CategorySalesData {
    category_id: ID
    category_name: String!
    quantity: Int!
    revenue: Int!
  }

  type CategorySalesPeriod {
    period: String!
    date: String!
    categories: [CategorySalesData!]!
    totalQuantity: Int!
    totalRevenue: Int!
  }

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
    categorySalesReport(
      period: String
      startDate: String
      endDate: String
    ): [CategorySalesPeriod!]!

    productSalesReport(
      period: String
      startDate: String
      endDate: String
      categoryId: ID
    ): [ProductSalesPeriod!]!

    revenueReport(
      period: String
      startDate: String
      endDate: String
    ): [RevenuePeriod!]!

    topSellingProductsReport(
      limit: Int
      startDate: String
      endDate: String
      categoryId: ID
    ): [TopProduct!]!

    salesOverview(
      startDate: String
      endDate: String
    ): SalesOverview!
  }
`;

module.exports = reportSchema;
