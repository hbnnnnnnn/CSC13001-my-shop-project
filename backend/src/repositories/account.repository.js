// chú ý sql injection, dùng $1, $2, ... và pg-format để tránh
const BaseRepository = require("./base.repository.js");
const db = require("../config/db.js");

class AccountRepository extends BaseRepository {
    constructor() {
        super("account", "account_id", db);
    }

    async findByUsername(username) {
        const result = await db.query(
            "SELECT * FROM account WHERE username = $1",
            [username]
        );

        return result.rows[0];
    }
}

module.exports = new AccountRepository();