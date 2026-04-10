// chú ý sql injection, dùng $1, $2, ... và pg-format để tránh
const BaseRepository = require("./base.repository.js");
const db = require("../config/db.js");
const format = require("pg-format");

class CustomerRepository extends BaseRepository {
    constructor() {
        super("customer", "customer_id", db);
    }

    async findByPhone(phone) {
        const query = format(
            "SELECT * FROM %I WHERE phone = $1",
            this.tableName
        );
        const result = await this.db.query(query, [phone]);
        return result.rows[0];
    }

    async findByEmail(email) {
        const query = format(
            "SELECT * FROM %I WHERE email = $1",
            this.tableName
        );
        const result = await this.db.query(query, [email]);
        return result.rows[0];
    }
}

module.exports = new CustomerRepository();