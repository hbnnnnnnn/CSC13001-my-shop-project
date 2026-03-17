// chú ý sql injection, dùng $1, $2, ... và pg-format để tránh
import BaseRepository from "./base.repository.js";
import db from "../config/db.js";

class ProductRepository extends BaseRepository {
    constructor() {
        super("product", "product_id", db);
    }

    async findByCategory(categoryId) {
        const result = await db.query(
            "SELECT * FROM product WHERE category_id = $1",
            [categoryId]
        );

        return result.rows;
    }

    async updateStock(productId, quantity) {
        const result = await db.query(
            `UPDATE product SET stock = $1 WHERE product_id = $2 RETURNING *`,
            [quantity, productId]
        );

        return result.rows[0];
    }
}

export default new ProductRepository();