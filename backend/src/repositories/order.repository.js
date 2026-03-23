// chú ý sql injection, dùng $1, $2, ... và pg-format để tránh
const BaseRepository = require("./base.repository.js");
const db = require("../config/db.js");

class OrderRepository extends BaseRepository {
    constructor() {
        super("orders", "order_id", db);
    }

    async getOrderItems(orderId) {
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