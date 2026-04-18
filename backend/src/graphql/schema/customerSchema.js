const customerSchema = `#graphql
  type Customer {
    customer_id: ID!
    name: String!
    phone: String
    email: String
    address: String
  }

  type CustomerList {
    data: [Customer!]!
    total: Int!
    page: Int!
    limit: Int!
    totalPages: Int!
  }

  type Query {
    customers(page: Int, limit: Int): CustomerList!
    customer(id: ID!): Customer
    customerByPhone(phone: String!): Customer
    customerByEmail(email: String!): Customer
  }

  type Mutation {
    createCustomer(name: String!, phone: String!, email: String, address: String): Customer!
    updateCustomer(id: ID!, name: String, phone: String, email: String, address: String): Customer!
    deleteCustomer(id: ID!): Boolean!
  }
`;

module.exports = customerSchema;
