require('dotenv').config();
const { ApolloServer } = require('@apollo/server');
const { expressMiddleware } = require('@apollo/server/express4');
const { ApolloServerPluginDrainHttpServer } = require('@apollo/server/plugin/drainHttpServer');
const http = require('http');
const app = require('./app');
const db = require('./config/db'); // Test DB connection early
const { typeDefs, resolvers } = require('./graphql');
const { getUserFromToken } = require('./middleware/auth.middleware');

const PORT = process.env.PORT || 4000;

async function startServer() {
    const httpServer = http.createServer(app);

    // Set up Apollo Server
    const server = new ApolloServer({
        typeDefs,
        resolvers,
        plugins: [ApolloServerPluginDrainHttpServer({ httpServer })],
    });

    await server.start();

    // Apply GraphQL middleware to Express app
    app.use(
        '/graphql',
        expressMiddleware(server, {
            context: async ({ req }) => {
                // Get user from token if Authorization header is provided
                const user = getUserFromToken(req);
                
                // Inject the 'db' pool and 'user' into context for all resolvers
                return { db, user };
            },
        })
    );

    // 404 handler — must be registered AFTER all routes (including /graphql)
    app.use((req, res) => {
        res.status(404).json({ error: `Route ${req.method} ${req.path} not found` });
    });
    await new Promise((resolve) => httpServer.listen({ port: PORT }, resolve));
    console.log(`Server ready at http://localhost:${PORT}/graphql`);
}

startServer().catch((error) => {
    console.error('Error starting Apollo Server:', error);
});