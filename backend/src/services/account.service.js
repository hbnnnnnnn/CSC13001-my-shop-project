const bcrypt = require('bcrypt');
const jwt = require('jsonwebtoken');

// Load dynamic import as a fallback in case repositories still use ES modules
// but we will write it with standard require if they convert it later.
// However, since we need to wait for dynamic import to initialize, it's a bit complex.
// Let's assume the user will convert them or I will convert them shortly, so let's use standard require.
// If it crashes, we'll fix the repos.
const accountRepository = require('../repositories/account.repository.js');

const generateToken = (account) => {
    const payload = {
        account_id: account.account_id,
        username: account.username,
        full_name: account.full_name,
        account_role: account.account_role
    };
    
    // Đọc JWT_SECRET từ .env, tạo expiry 7d
    const secret = process.env.JWT_SECRET || 'your_super_secret_key_here';
    return jwt.sign(payload, secret, { expiresIn: process.env.JWT_EXPIRES_IN || '7d' });
};

const login = async (username, password) => {
    // 1. Kiểm tra username có tồn tại không
    // Chú ý: Vì accountRepository có thể được export bằng `export default`, khi require có thể nằm trong field `.default`
    const repo = accountRepository;
    
    const account = await repo.findByUsername(username);
    if (!account) {
        throw new Error('Sai tài khoản hoặc mật khẩu');
    }

    // 2. So sánh mật khẩu bằng bcrypt
    const isValid = await bcrypt.compare(password, account.password_hash);
    if (!isValid) {
        throw new Error('Sai tài khoản hoặc mật khẩu');
    }

    // 3. Tạo token và trả ra
    const token = generateToken(account);
    return {
        token,
        account
    };
};

const register = async (username, password, full_name, account_role) => {
    const repo = accountRepository;
    
    // 1. Kiểm tra xem username đã tồn tại chưa
    const existing = await repo.findByUsername(username);
    if (existing) {
        throw new Error('Tài khoản này đã tồn tại');
    }

    // 2. Băm mật khẩu
    const password_hash = await bcrypt.hash(password, 10);

    // 3. Lưu vào database
    const newAccount = await repo.create({
        username,
        password_hash,
        full_name,
        account_role: account_role || 'Sale' // Mặc định là Sale
    });

    // 4. Sinh JWT Token để tự đăng nhập luôn sau khi đăng ký thành công
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
