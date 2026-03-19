const bcrypt = require('bcrypt');
const jwt = require('jsonwebtoken');

const accountRepository = require('../repositories/account.repository.js');

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

    const account = await repo.findByUsername(username);
    if (!account) {
        throw new Error('Invalid username');
    }

    const isValid = await bcrypt.compare(password, account.password_hash);
    if (!isValid) {
        throw new Error('Invalid password');
    }

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

module.exports = {
    login,
    register
};
