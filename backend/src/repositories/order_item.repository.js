// chú ý sql injection, dùng $1, $2, ... và pg-format để tránh
const BaseRepository = require("./base.repository.js");
const db = require("../config/db.js");

class OrderItemRepository extends BaseRepository {
    constructor() {
        super("order_item", "order_item_id", db);
    }

    async findByOrderId(orderId, client) {
        const db = client || this.db;
        const result = await db.query(
            "SELECT * FROM order_item WHERE order_id = $1",
            [orderId]
        );

        return result.rows;
    }

    async findByOrderIds(orderIds, client) {
        const db = client || this.db;
        const result = await db.query(
            "SELECT * FROM order_item WHERE order_id = ANY($1)",
            [orderIds]
        );

        return result.rows;
    }
}

module.exports = new OrderItemRepository();
