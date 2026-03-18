const jwt = require('jsonwebtoken');

const verifyToken = (token) => {
    try {
        if (!token) return null;
        const secret = process.env.JWT_SECRET || 'your_jwt_secret';
        return jwt.verify(token, secret);
    } catch (error) {
        return null; // Token hết hạn hoặc sai
    }
};

const getUserFromToken = (req) => {
    const authHeader = req.headers.authorization;
    if (authHeader && authHeader.startsWith('Bearer ')) {
        const token = authHeader.split(' ')[1];
        return verifyToken(token);
    }
    return null;
};

// Wrapper bắt buộc phải đăng nhập mới được xài Resolver
const requireAuth = (resolverFunc) => {
    return (parent, args, context, info) => {
        if (!context.user) {
            throw new Error('Unauthenticated: You are not logged in');
        }
        return resolverFunc(parent, args, context, info);
    };
};

// Wrapper bắt buộc phải có Role theo danh sách truyền vào
// Ví dụ: requireRole(['Admin', 'Sale'], hamsuly_gi_do)
const requireRole = (allowedRoles, resolverFunc) => {
    return (parent, args, context, info) => {
        if (!context.user) {
            throw new Error('Unauthenticated: You are not logged in');
        }

        if (!allowedRoles.includes(context.user.account_role)) {
            throw new Error('Unauthorized: You do not have permission to access');
        }

        return resolverFunc(parent, args, context, info);
    };
};

module.exports = {
    getUserFromToken,
    requireAuth,
    requireRole
};
