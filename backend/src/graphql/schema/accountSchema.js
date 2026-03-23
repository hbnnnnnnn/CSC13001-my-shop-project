const accountSchema = `#graphql
  type Account {
    account_id: ID!
    username: String!
    full_name: String!
    account_role: String!
  }

  type AuthPayload {
    token: String!
    account: Account!
  }

  type Query {
    me: Account
  }

  type Mutation {
    login(username: String!, password: String!): AuthPayload!
    register(username: String!, password: String!, full_name: String!, account_role: String): AuthPayload!
  }
`;

module.exports = accountSchema;
