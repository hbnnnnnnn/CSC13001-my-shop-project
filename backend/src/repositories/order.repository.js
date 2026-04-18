// chú ý sql injection, dùng $1, $2, ... và pg-format để tránh
const BaseRepository = require("./base.repository.js");
const db = require("../config/db.js");

const format = require("pg-format");

const SORT_FIELD_MAP = {
  CREATED_TIME: "created_time",
  FINAL_PRICE: "final_price",
};

class OrderRepository extends BaseRepository {
    constructor() {
        super("orders", "order_id", db, true); // true = use soft delete
    }

    async findAllFiltered({ page = 1, limit = 10, filter = {}, sort = {} } = {}, client) {
        const db = client || this.db;
        const offset = (page - 1) * limit;
        const conditions = [];
        const params = [];

        // Base Soft Delete condition
        conditions.push("is_deleted = false");

        // Build WHERE conditions
        if (filter.status) {
            params.push(filter.status);
            conditions.push(`status = $${params.length}`);
        }
        if (filter.startDate) {
            params.push(filter.startDate);
            conditions.push(`created_time >= $${params.length}`);
        }
        if (filter.endDate) {
            params.push(filter.endDate);
            conditions.push(`created_time <= $${params.length}`);
        }

        const whereClause = conditions.length > 0 ? "WHERE " + conditions.join(" AND ") : "";

        // Build ORDER BY clause
        let orderClause = "";
        if (sort && sort.field && sort.order) {
            const column = SORT_FIELD_MAP[sort.field];
            if (column) {
                orderClause = format("ORDER BY %I %s", column, sort.order);
            }
        } else {
            // Default sort: newest first
            orderClause = "ORDER BY created_time DESC";
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

    async getOrderItems(orderId, client) {
        const db = client || this.db;
        const result = await db.query(
            `SELECT *
       FROM order_item
       WHERE order_id = $1`,
            [orderId]
        );

        return result.rows;
    }
}

module.exports = new OrderRepository();
