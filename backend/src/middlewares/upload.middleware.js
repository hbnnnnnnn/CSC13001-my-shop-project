const multer = require("multer");

// dùng RAM để giữ file tạm
const storage = multer.memoryStorage();

// thêm giới hạn file
const upload = multer({
    storage,
    limits: {
        fileSize: 5 * 1024 * 1024, // 5MB
    },
});

module.exports = upload;