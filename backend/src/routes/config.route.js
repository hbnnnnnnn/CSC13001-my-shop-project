const express = require('express');
const router = express.Router();
const db = require('../config/db');
const { restRoleMiddleware } = require('../middlewares/auth.middleware');

// POST /api/config/db
// Cập nhật cấu hình database
router.post('/db', restRoleMiddleware(['Admin']), async (req, res) => {
    try {
        const { host, port, user, password, database } = req.body;

        // Kiểm tra xem các trường có được gửi lên không
        if (!host || !port || !user || !database) {
            return res.status(400).json({
                error: 'missing field (host, port, user, database)'
            });
        }

        const newConfig = {
            host,
            port: Number(port),
            user,
            password: password || '', // PostgreSQL allow empty password sometimes, but usually FE sends string
            database
        };

        await db.updateConfig(newConfig);

        return res.status(200).json({
            message: 'Update db success'
        });
    } catch (error) {
        console.error('Error when config db:', error);
        return res.status(400).json({
            error: error.message || 'Cannot connect to new db'
        });
    }
});

module.exports = router;
