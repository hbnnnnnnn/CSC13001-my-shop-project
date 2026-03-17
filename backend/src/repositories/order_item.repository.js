// chú ý sql injection, dùng $1, $2, ... và pg-format để tránh
import BaseRepository from "./base.repository.js";
import db from "../config/db.js";

class OrderItemRepository extends BaseRepository {
    constructor() {
        super("order_item", "order_item_id", db);
    }

    async findByOrderId(orderId) {
        const result = await db.query(
            "SELECT * FROM order_item WHERE order_id = $1",
            [orderId]
        );

        return result.rows;
    }
}

export default new OrderItemRepository();
