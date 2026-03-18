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

  input CreateProductInput {
    sku: String!
    name: String!
    price: Int!
    stock: Int!
    description: String
    images: [String]
    supplier: String
    category_id: ID!
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
  }



  extend type Query {
    products: [Product!]!
    product(id: ID!): Product
  }

  extend type Mutation { 
    createProduct(input: CreateProductInput!): Product!
    updateProduct(id: ID!, input: UpdateProductInput!): Product!
    deleteProduct(id: ID!): Boolean!
  }
`;

module.exports = productSchema;
