const fs = require('fs');
const path = require('path');
const db = require('../config/db');

async function runSQLFile(filePath) {
    try {
        const fullPath = path.join(__dirname, filePath);
        const sql = fs.readFileSync(fullPath, 'utf8');
        if (sql.trim() === '') return;

        console.log(`Executing ${filePath}...`);
        await db.query(sql);
        console.log(`Done executing ${filePath}`);
    } catch (err) {
        console.error(`Error executing ${filePath}:`, err);
        throw err;
    }
}

async function initDb() {
    try {
        // Chạy file tạo bảng trước (Migrations)
        await runSQLFile('../../database/migrations/01_init_schema.sql');

        // Chạy file chèn dữ liệu mẫu (Seeds)
        await runSQLFile('../../database/seeds/01_dummy_data.sql');

        console.log('--- Database Initialization Completed Successfully! ---');
    } catch (error) {
        console.error('--- Failed to initialize database ---');
    } finally {
        // Phải exit để terminal không bị treo do connection pool vẫn còn mở
        process.exit(0);
    }
}

initDb();
