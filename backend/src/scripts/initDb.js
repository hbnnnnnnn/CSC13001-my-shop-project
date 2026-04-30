require('dotenv').config();
const fs = require('fs');
const path = require('path');
const db = require('../config/db');

/**
 * Script này thực hiện việc quét và chạy các file SQL trong thư mục database/migrations và database/seeds.
 * Nó sử dụng bảng 'schema_migrations' để ghi nhớ các file đã chạy, tránh việc chạy trùng lặp gây lỗi.
 */

async function ensureMigrationTable() {
    await db.query(`
        CREATE TABLE IF NOT EXISTS schema_migrations (
            id SERIAL PRIMARY KEY,
            filename VARCHAR(255) UNIQUE NOT NULL,
            executed_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
        );
    `);
}

async function runSQLFile(fullPath) {
    const filename = path.basename(fullPath);
    const sql = fs.readFileSync(fullPath, 'utf8');

    if (sql.trim() === '') return;

    const client = await db.pool.connect();
    try {
        await client.query('BEGIN');

        console.log(`[Migration] Processing: ${filename}...`);
        await client.query(sql);

        // Ghi lại lịch sử migration
        await client.query('INSERT INTO schema_migrations (filename) VALUES ($1)', [filename]);

        await client.query('COMMIT');
        console.log(`[Migration] Done processing: ${filename}`);
    } catch (err) {
        await client.query('ROLLBACK');
        console.error(`[Migration] ERROR processing file ${filename}:`, err.message);
        throw err;
    } finally {
        client.release();
    }
}

async function migrate() {
    try {
        console.log('--- START MIGRATION PROCESS ---');

        await ensureMigrationTable();

        // 1. Lấy danh sách các file đã chạy
        const { rows } = await db.query('SELECT filename FROM schema_migrations');
        const executedFiles = new Set(rows.map(r => r.filename));

        // 2. Xác định các thư mục cần quét
        const folders = [
            { path: '../../database/migrations', label: 'MIGRATIONS' },
            { path: '../../database/seeds', label: 'SEEDS' }
        ];

        for (const folder of folders) {
            const folderPath = path.join(__dirname, folder.path);

            if (!fs.existsSync(folderPath)) continue;

            // Đọc các file .sql và sắp xếp theo tên
            const files = fs.readdirSync(folderPath)
                .filter(f => f.endsWith('.sql'))
                .sort();

            for (const file of files) {
                if (!executedFiles.has(file)) {
                    await runSQLFile(path.join(folderPath, file));
                } else {
                    console.log(`[Migration] Already processed (Skipping): ${file}`);
                }
            }
        }

        console.log('--- ALL MIGRATION DONE SUCCESSFULY ---');
    } catch (error) {
        console.error('--- MIGRATION PROCESS FAILED ---');
        console.error(error);
        process.exit(1);
    } finally {
        process.exit(0);
    }
}

migrate();
