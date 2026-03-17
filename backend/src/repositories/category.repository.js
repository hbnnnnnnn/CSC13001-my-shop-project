// chú ý sql injection, dùng $1, $2, ... và pg-format để tránh
import BaseRepository from "./base.repository.js";
import db from "../config/db.js";

class CategoryRepository extends BaseRepository {
    constructor() {
        super("category", "category_id", db);
    }
}

export default new CategoryRepository();