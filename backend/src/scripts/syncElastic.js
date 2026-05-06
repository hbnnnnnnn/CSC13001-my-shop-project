require("dotenv").config();

const esClient = require("../config/elasticsearch");
const db = require("../config/db");
const searchService = require("../services/search.service");
const { redisClient } = require("../config/redis");

const INDEX = process.env.ELASTICSEARCH_PRODUCT_INDEX || "products";

const init = async () => {
  // Connect to Redis for cache operations during sync
  if (!redisClient.isOpen) {
    await redisClient.connect();
  }

  // Skip sync if index already exists (data was already synced before)
  if (await esClient.indices.exists({ index: INDEX })) {
    console.log(
      `Elasticsearch index "${INDEX}" already exists, skipping sync.`,
    );
    return;
  }

  await searchService.createIndex();

  // Bulk index all products from Postgres
  const result = await db.query(`SELECT p.*, c.name AS category_name 
    FROM product p
    LEFT JOIN category c ON p.category_id = c.category_id
    `);
  const products = result.rows;

  console.log(`Syncing ${products.length} products to Elasticsearch...`);
  await searchService.bulkIndexProducts(products);
  console.log("Elasticsearch sync complete.");
};

init()
  .then(() => {
    process.exit(0);
  })
  .catch((err) => {
    console.error("Sync failed:", err);
    process.exit(1);
  });

