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
  }

  type Query {
    products: [Product!]!
  }

  type Mutation {
    _emptyProductMutation: String
  }
`;

module.exports = productSchema;
