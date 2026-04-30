const express = require('express');
const cors = require('cors');
const uploadRoute = require("./routes/upload.route.js");
const importRoute = require("./routes/import.route.js");

const app = express();

// Global middleware
app.use(cors());                 // Will be narrowed to /graphql only later if needed
app.use(express.json());         // Parse JSON bodies for any REST routes
app.use("/api/upload", uploadRoute);
app.use("/api/import", importRoute);

// REST routes (lightweight, non-GraphQL)
app.get('/health', (req, res) => {
    res.status(200).json({ status: 'ok', message: 'MyShop Backend is running!' });
});

module.exports = app;