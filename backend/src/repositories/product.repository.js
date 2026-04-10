// chú ý sql injection, dùng $1, $2, ... và pg-format để tránh
const BaseRepository = require("./base.repository.js");
const format = require("pg-format");
const db = require("../config/db.js");

// Map GraphQL SortField enum values to actual DB column names
const SORT_FIELD_MAP = {
  PRICE: "price",
  NAME: "name",
  STOCK: "stock",
  CREATED_AT: "created_time",
  UPDATED_AT: "updated_time",
};

class ProductRepository extends BaseRepository {
  constructor() {
    super("product", "product_id", db);
  }

  async findAllFiltered({ page = 1, limit = 10, filter = {}, sort = {} } = {}, client) {
    const db = client || this.db;
    const offset = (page - 1) * limit;
    const conditions = [];
    const params = [];

    // Build WHERE conditions
    if (filter.category_id) {
      params.push(filter.category_id);
      conditions.push(`category_id = $${params.length}`);
    }
    if (filter.min_price != null) {
      params.push(filter.min_price);
      conditions.push(`price >= $${params.length}`);
    }
    if (filter.max_price != null) {
      params.push(filter.max_price);
      conditions.push(`price <= $${params.length}`);
    }

    const whereClause =
      conditions.length > 0 ? "WHERE " + conditions.join(" AND ") : "";

    // Build ORDER BY clause
    let orderClause = "";
    if (sort && sort.field && sort.order) {
      const column = SORT_FIELD_MAP[sort.field];
      if (column) {
        orderClause = format("ORDER BY %I %s", column, sort.order);
      }
    }

    // Build data + count queries
    const dataQuery =
      format("SELECT * FROM %I", this.tableName) +
      ` ${whereClause} ${orderClause} LIMIT $${params.length + 1} OFFSET $${params.length + 2}`;

    const countQuery =
      format("SELECT COUNT(*) FROM %I", this.tableName) + ` ${whereClause}`;

    const [dataResult, countResult] = await Promise.all([
      db.query(dataQuery, [...params, limit, offset]),
      db.query(countQuery, params),
    ]);

    const total = parseInt(countResult.rows[0].count);

    return {
      data: dataResult.rows,
      total,
      page,
      limit,
      totalPages: Math.ceil(total / limit),
    };
  }

  async findByIdWithCategory(productId, client) {
    const db = client || this.db;
    const result = await db.query(
      `SELECT p.*, c.name AS category_name 
      FROM product p
      LEFT JOIN category c ON p.category_id = c.category_id
      WHERE p.product_id = $1`,
      [productId],
    );

    return result.rows[0];
  }

  async findByCategory(categoryId, client) {
    const db = client || this.db;
    const result = await db.query(
      "SELECT * FROM product WHERE category_id = $1",
      [categoryId],
    );

    return result.rows;
  }

  async updateStock(productId, quantity, client) {
    const db = client || this.db;
    const result = await db.query(
      `UPDATE product SET stock = $1 WHERE product_id = $2 RETURNING *`,
      [quantity, productId],
    );

    return result.rows[0];
  }

  async findTopLowStockProducts(limit = 5, client) {
    const db = client || this.db;
    const result = await db.query(
      `SELECT * FROM product ORDER BY stock ASC LIMIT $1`,
      [limit],
    );

    return result.rows;
  }

  async findTopSellingProducts(limit = 5, client) {
    const db = client || this.db;
    const result = await db.query(
      `SELECT p.*, SUM(oi.quantity) as total_sold
            FROM product p
            JOIN order_item oi ON p.product_id = oi.product_id
            GROUP BY p.product_id
            ORDER BY total_sold DESC
            LIMIT $1`,
      [limit],
    );

    return result.rows;
  }
  // dùng để tìm ids để update stock trong order service, dùng lệnh FOR UPDATE để khóa các dòng
  async findByIdsForUpdate(ids, client) {
    const db = client || this.db;
    if (!ids || ids.length === 0) return [];

    // Sort IDs to prevent deadlocks when locking multiple rows concurrently
    const sortedIds = [...new Set(ids)].sort((a, b) => a - b);

    const result = await db.query(
      'SELECT * FROM product WHERE product_id = ANY($1) ORDER BY product_id FOR UPDATE',
      [sortedIds]
    );

    return result.rows;
  }
}

module.exports = new ProductRepository();

