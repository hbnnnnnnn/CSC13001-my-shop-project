const orderSchema = `#graphql
  type OrderItem {
    order_item_id: ID!
    order_id: ID!
    product_id: ID!
    quantity: Int!
    unit_sale_price: Int!
    total_price: Int!
  }

  type Order {
    order_id: ID!
    created_time: String!
    updated_time: String!
    final_price: Int!
    status: String!
    customer_id: ID
    account_id: ID
    shipping_address: String
    items: [OrderItem!]
  }

  type OrderList {
    data: [Order!]!
    total: Int!
    page: Int!
    limit: Int!
    totalPages: Int!
  }

  input OrderItemInput {
    product_id: ID!
    quantity: Int!
  }

  type Query {
    orders(page: Int, limit: Int): OrderList!
    order(id: ID!): Order
  }

  type Mutation {
    createOrder(
      customer_id: ID
      account_id: ID
      shipping_address: String
      items: [OrderItemInput!]!
    ): Order!
    
    updateOrderStatus(id: ID!, status: String!): Order!
  }
`;

module.exports = orderSchema;
