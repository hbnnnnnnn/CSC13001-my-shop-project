const { Pool } = require("pg");
const fs = require("fs");
const path = require("path");

const CONFIG_PATH = path.join(__dirname, "db.config.json");

let pool;

function loadConfig() {
  let config = {
    host: process.env.DB_HOST || "localhost",
    port: process.env.DB_PORT || 5432,
    user: process.env.DB_USER || "postgres",
    password: process.env.DB_PASSWORD || "postgres",
    database: process.env.DB_NAME || "myshop",
  };

  try {
    if (fs.existsSync(CONFIG_PATH)) {
      const fileData = fs.readFileSync(CONFIG_PATH, "utf-8");
      const parsedConfig = JSON.parse(fileData);
      config = { ...config, ...parsedConfig };
      console.log("Loaded DB config from db.config.json");
    } else {
      console.log("Using DB config from environment variables");
    }
  } catch (error) {
    console.error(
      "Error reading db.config.json, using fallback config:",
      error.message,
    );
  }

  return config;
}

function initPool() {
  const config = loadConfig();
  pool = new Pool(config);

  pool.connect((err, client, release) => {
    if (err) {
      console.error("Error connecting to PostgreSQL database:", err.stack);
    } else {
      console.log("Connected to PostgreSQL database");
      release();
    }
  });
}

// Initial connection
initPool();

const updateConfig = async (newConfig) => {
  const testPool = new Pool(newConfig);
  try {
    // Test connection first
    const client = await testPool.connect();
    client.release();

    // Save to file
    fs.writeFileSync(CONFIG_PATH, JSON.stringify(newConfig, null, 2), "utf-8");

    // Switch pool
    const oldPool = pool;
    pool = testPool;

    if (oldPool) {
      oldPool.end().catch((e) => console.error("Error closing old pool:", e));
    }
    console.log("DB configuration updated successfully");
  } catch (err) {
    testPool.end().catch(() => {});
    console.error(
      "Failed to update DB configuration. Keeping the old connection pool.",
    );
    throw new Error(
      "Could not connect to database with new config: " + err.message,
    );
  }
};

module.exports = {
  get pool() {
    return pool;
  },
  query: (text, params) => pool.query(text, params),
  updateConfig,
};

