const categorySchema = `#graphql
  type Category {
    category_id: ID!
    name: String!
    description: String
  }

  type CategoryList {
    data: [Category!]!
    total: Int!
    page: Int!
    limit: Int!
    totalPages: Int!
  }

  type Query {
    categories(page: Int, limit: Int): CategoryList!
    category(id: ID!): Category
  }

  type Mutation {
    createCategory(name: String!, description: String): Category!
    updateCategory(id: ID!, name: String, description: String): Category!
    deleteCategory(id: ID!): Boolean!
  }
`;

module.exports = categorySchema;
