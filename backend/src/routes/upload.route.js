const express = require('express');
const upload = require("../middlewares/upload.middleware.js");
const { uploadImage } = require("../services/upload.service.js");

const router = express.Router();

// POST /api/upload
router.post("/", upload.single("image"), async (req, res) => {
    try {
        if (!req.file) {
            return res.status(400).json({ message: "No file uploaded" });
        }

        const imageUrl = await uploadImage(req.file.buffer);

        return res.json({
            message: "Upload success",
            imageUrl,
        });
    } catch (error) {
        return res.status(500).json({
            message: "Upload failed",
            error: error.message,
        });
    }
});

module.exports = router;