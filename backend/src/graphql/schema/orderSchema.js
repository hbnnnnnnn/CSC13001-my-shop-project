const orderSchema = `#graphql
  type Order {
    order_id: ID!
    final_price: Int!
    status: String!
    shipping_address: String
    # ... other fields
  }

  type Query {
    orders: [Order!]!
  }

  type Mutation {
    _emptyOrderMutation: String
  }
`;

module.exports = orderSchema;
