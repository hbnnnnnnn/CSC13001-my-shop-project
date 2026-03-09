const accountSchema = `#graphql
  type Account {
    account_id: ID!
    username: String!
    full_name: String!
    account_role: String!
  }

  type Query {
    # e.g., getAccounts: [Account!]!
    _emptyAccountQuery: String
  }

  type Mutation {
    # e.g., login, createAccount
    _emptyAccountMutation: String
  }
`;

module.exports = accountSchema;
