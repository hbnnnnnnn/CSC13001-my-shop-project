const productSchema = `#graphql
  type Category {
    category_id: ID!
    name: String!
    description: String
  }

  type Product {
    product_id: ID!
    sku: String!
    name: String!
    price: Int!
    stock: Int!
    description: String
    images: [String]
    supplier: String
    category: Category
    created_time: String
    updated_time: String
  }

  type TopSellingProduct {
    product_id: ID!
    sku: String!
    name: String!
    price: Int!
    stock: Int!
    description: String
    images: [String]
    supplier: String
    category: Category
    created_time: String
    updated_time: String
    total_sold: Int!
  }

  type ProductPage {
    data: [Product!]!
    total: Int!
    page: Int!
    limit: Int!
    totalPages: Int!
  }

  type SearchResult {
    data: [Product!]!
    total: Int!
    page: Int!
    limit: Int!
    totalPages: Int!
  }

  input CreateProductInput {
    sku: String!
    name: String!
    price: Int!
    stock: Int!
    description: String
    images: [String]
    supplier: String
    category_id: ID!
    cost_price: Int
  }

  input UpdateProductInput {
    sku: String
    name: String
    price: Int
    stock: Int
    description: String
    images: [String]
    supplier: String
    category_id: ID
    cost_price: Int
  }

  input ProductFilter {
    category_id: ID
    min_price: Int
    max_price: Int
  }

  enum SortField {
    PRICE
    NAME
    STOCK
    CREATED_AT
    UPDATED_AT
  }

  enum SortOrder {
    ASC
    DESC
  }

  input ProductSort {
    field: SortField!
    order: SortOrder!
  }

  extend type Query {
    products(page: Int, limit: Int, filter: ProductFilter, sort: ProductSort): ProductPage!
    product(id: ID!): Product
    topLowStockProducts(limit: Int): [Product!]!
    topSellingProducts(limit: Int): [TopSellingProduct!]!
    productSearch(query: String!, page: Int, limit: Int, filter: ProductFilter, sort: ProductSort): SearchResult!
  }

  extend type Mutation { 
    createProduct(input: CreateProductInput!): Product!
    updateProduct(id: ID!, input: UpdateProductInput!): Product!
    deleteProduct(id: ID!): Boolean!
  }
  
  type AIProductSuggestion {
    name: String
    description: String
  }

  extend type Mutation {
    generateProductDetailsFromImage(imageUrl: String!): AIProductSuggestion!
  }
`;

module.exports = productSchema;

