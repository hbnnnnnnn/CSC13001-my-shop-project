const express = require('express');
const cors = require('cors');

const app = express();

// Global middleware
app.use(cors());                 // Will be narrowed to /graphql only later if needed
app.use(express.json());         // Parse JSON bodies for any REST routes

// REST routes (lightweight, non-GraphQL)
app.get('/health', (req, res) => {
    res.status(200).json({ status: 'ok', message: 'MyShop Backend is running!' });
});

module.exports = app;