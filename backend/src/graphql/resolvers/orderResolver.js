const orderResolver = {
    Query: {
        orders: async (_, args, { db }) => {
            // Placeholder
            return [];
        }
    },
    Mutation: {
        // createOrder: async (_, args, context) => { ... }
    }
};

module.exports = orderResolver;
