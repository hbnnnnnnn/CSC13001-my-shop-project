const accountService = require('../../services/account.service.js');
const { requireAuth } = require('../../middlewares/auth.middleware.js');
const cacheService = require('../../utils/cache.util.js');
const jwt = require('jsonwebtoken');

const accountResolver = {
    Query: {
        me: requireAuth(async (_, args, context) => {
            // Nhờ requireAuth bọc ngoài, nếu code chạy tới đây thì chắc chắn context.user có sẵn và chính xác
            return context.user;
        })
    },
    Mutation: {
        login: async (_, { username, password }) => {
            return await accountService.login(username, password);
        },
        register: async (_, { username, password, full_name, account_role }) => {
            return await accountService.register(username, password, full_name, account_role);
        },
        logout: requireAuth(async (_, args, { token }) => {
            if (!token) return true;
            try {
                // Decode without verifying to get the 'exp' (expiration time)
                const decoded = jwt.decode(token);
                if (decoded && decoded.exp) {
                    // Calculate remaining seconds until token expires
                    const currentUnixTime = Math.floor(Date.now() / 1000);
                    const ttlSeconds = decoded.exp - currentUnixTime;

                    if (ttlSeconds > 0) {
                        // Store token in blacklist with standard TTL
                        await cacheService.set(`blacklist:${token}`, 'true', ttlSeconds);
                        console.log(`[Logout] Token blacklisted for ${ttlSeconds}s`);
                    }
                }
            } catch (err) {
                console.error("Logout error:", err);
            }
            return true;
        })
    }
};

module.exports = accountResolver;
