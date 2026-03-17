// chú ý sql injection, dùng $1, $2, ... và pg-format để tránh
import BaseRepository from "./base.repository.js";
import db from "../config/db.js";

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

export default new AccountRepository();