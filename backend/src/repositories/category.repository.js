// chú ý sql injection, dùng $1, $2, ... và pg-format để tránh
const BaseRepository = require("./base.repository.js");
const db = require("../config/db.js");
const format = require("pg-format");

class CategoryRepository extends BaseRepository {
    constructor() {
        super("category", "category_id", db);
    }

    async findByName(name) {
        const query = format(
            "SELECT * FROM %I WHERE name = $1",
            this.tableName
        );
        const result = await this.db.query(query, [name]);
        return result.rows[0];
    }
}

module.exports = new CategoryRepository();