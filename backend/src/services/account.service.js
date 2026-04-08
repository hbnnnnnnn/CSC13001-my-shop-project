const bcrypt = require('bcrypt');
const jwt = require('jsonwebtoken');

const accountRepository = require('../repositories/account.repository.js');
const cacheService = require("../utils/cache.util.js");

const MAX_LOGIN_ATTEMPTS = 5;
const LOCKOUT_MINUTES = 1;

const generateToken = (account) => {
    const payload = {
        account_id: account.account_id,
        username: account.username,
        full_name: account.full_name,
        account_role: account.account_role
    };

    const secret = process.env.JWT_SECRET || 'your_super_secret_key_here';
    return jwt.sign(payload, secret, { expiresIn: process.env.JWT_EXPIRES_IN || '7d' });
};

const login = async (username, password) => {
    const repo = accountRepository;

    // 1. Check Rate Limiter
    const rateLimitKey = `ratelimit:login:${username}`;
    let attempts = await cacheService.get(rateLimitKey) || 0;

    if (attempts >= MAX_LOGIN_ATTEMPTS) {
        throw new Error(`Too many attempts, please try again in ${LOCKOUT_MINUTES} minutes`);
    }

    const account = await repo.findByUsername(username);
    if (!account) {
        // Increment failed attempts even if username doesn't exist to prevent enumeration
        await cacheService.set(rateLimitKey, attempts + 1, LOCKOUT_MINUTES * 60);
        throw new Error('Invalid username');
    }

    const isValid = await bcrypt.compare(password, account.password_hash);
    if (!isValid) {
        // Increment failed attempts
        await cacheService.set(rateLimitKey, attempts + 1, LOCKOUT_MINUTES * 60);
        throw new Error('Invalid password');
    }

    // Success! Reset attempts.
    await cacheService.del(rateLimitKey);

    const token = generateToken(account);
    return {
        token,
        account
    };
};

const register = async (username, password, full_name, account_role) => {
    const repo = accountRepository;

    const existing = await repo.findByUsername(username);
    if (existing) {
        throw new Error('Username already exists');
    }

    const password_hash = await bcrypt.hash(password, 10);

    const newAccount = await repo.create({
        username,
        password_hash,
        full_name,
        account_role: account_role || 'Sale'
    });

    const token = generateToken(newAccount);
    return {
        token,
        account: newAccount
    };
};

const logout = async (token) => {
    if (!token) return true;
    try {
        // Giải mã không cần verify để lấy thời gian expire (exp)
        const decoded = jwt.decode(token);
        if (decoded && decoded.exp) {
            const currentUnixTime = Math.floor(Date.now() / 1000);
            const ttlSeconds = decoded.exp - currentUnixTime;

            if (ttlSeconds > 0) {
                // Đưa token vào blacklist trong Redis với TTL khớp với thời gian còn lại của token
                await cacheService.set(`blacklist:${token}`, 'true', ttlSeconds);
                console.log(`[Logout Service] Token blacklisted for ${ttlSeconds}s`);
            }
        }
    } catch (err) {
        console.error("Logout Service Error:", err);
    }
    return true;
};

module.exports = {
    login,
    register,
    logout
};
