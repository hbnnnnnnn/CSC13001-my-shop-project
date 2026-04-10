const orderSchema = `#graphql
  type OrderItem {
    order_item_id: ID!
    order_id: ID!
    product_id: ID!
    quantity: Int!
    unit_sale_price: Int!
    total_price: Int!
    product: Product
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
    recipient_name: String
    recipient_phone: String
    recipient_email: String
    items: [OrderItem!]
    customer: Customer
    account: Account
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

  input UpdateOrderInput {
    shipping_address: String
    recipient_name: String
    recipient_phone: String
    recipient_email: String
  }

  input UpdateOrderFullInput {
    shipping_address: String
    recipient_name: String
    recipient_phone: String
    recipient_email: String
    status: String
    items: [OrderItemInput!]
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
      recipient_name: String
      recipient_phone: String
      recipient_email: String
      items: [OrderItemInput!]!
    ): Order!
    
    updateOrderStatus(id: ID!, status: String!): Order! @deprecated(reason: "Use updateOrderFull instead")
    updateOrder(id: ID!, input: UpdateOrderInput!): Order! @deprecated(reason: "Use updateOrderFull instead")
    updateOrderFull(id: ID!, input: UpdateOrderFullInput!): Order!
    deleteOrder(id: ID!): Boolean!
  }
`;

module.exports = orderSchema;
