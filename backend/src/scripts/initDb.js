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
        // Only re-seed data (schema is auto-created by Postgres initdb.d on first run)
        // This is useful when you want to reset data without destroying the volume
        await runSQLFile('../../database/seeds/01_dummy_data.sql');

        console.log('--- Seed data loaded successfully! ---');
    } catch (error) {
        console.error('--- Failed to seed database ---', error.message);
    } finally {
        process.exit(0);
    }
}

initDb();
