// chú ý sql injection, dùng $1, $2, ... và pg-format để tránh
const BaseRepository = require("./base.repository.js");
const db = require("../config/db.js");

class CustomerRepository extends BaseRepository {
    constructor() {
        super("customer", "customer_id", db);
    }
}

module.exports = new CustomerRepository();