const express = require("express");
const multer = require("multer");
const { importProductsFromExcel } = require("../services/import.service.js");
const { getUserFromToken } = require("../middlewares/auth.middleware.js");

const router = express.Router();

// Multer config for Excel files — memory storage, 10 MB limit
const upload = multer({
  storage: multer.memoryStorage(),
  limits: { fileSize: 10 * 1024 * 1024 }, // 10 MB
  fileFilter: (req, file, cb) => {
    // Accept only .xlsx files
    if (
      file.mimetype ===
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" ||
      file.originalname.endsWith(".xlsx")
    ) {
      cb(null, true);
    } else {
      cb(new Error("Only .xlsx files are allowed"), false);
    }
  },
});

// POST /api/import/products
router.post("/products", upload.single("file"), async (req, res) => {
  try {
    // 1. Auth check — require Bearer token with Admin or Sale role
    const { user } = await getUserFromToken(req);
    if (!user) {
      return res
        .status(401)
        .json({ message: "Unauthenticated: You are not logged in" });
    }

    const allowedRoles = ["Admin", "Sale"];
    if (!allowedRoles.includes(user.account_role)) {
      return res
        .status(403)
        .json({
          message: "Unauthorized: You do not have permission to access",
        });
    }

    // 2. File validation
    if (!req.file) {
      return res
        .status(400)
        .json({
          message: 'No file uploaded. Send a .xlsx file in the "file" field.',
        });
    }

    // 3. Run import
    const result = await importProductsFromExcel(req.file.buffer);

    return res.json({
      message: "Import completed",
      ...result,
    });
  } catch (error) {
    console.error("[Import] Error:", error.message);
    return res.status(500).json({
      message: "Import failed",
      error: error.message,
    });
  }
});

module.exports = router;

