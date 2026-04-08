const accountService = require('../../services/account.service.js');
const { requireAuth } = require('../../middlewares/auth.middleware.js');

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
            return await accountService.logout(token);
        })
    }
};

module.exports = accountResolver;
