// chú ý sql injection, dùng $1, $2, ... và pg-format để tránh
const format = require('pg-format');

class BaseRepository {
  constructor(tableName, idColumn, db) {
    this.tableName = tableName;
    this.idColumn = idColumn;
    this.db = db;
  }

  async findAll({ page = 1, limit = 10 } = {}, client) {
    const db = client || this.db;
    const offset = (page - 1) * limit;

    const dataQuery = format(
      'SELECT * FROM %I LIMIT $1 OFFSET $2',
      this.tableName
    );

    const countQuery = format(
      'SELECT COUNT(*) FROM %I',
      this.tableName
    );

    const [dataResult, countResult] = await Promise.all([
      db.query(dataQuery, [limit, offset]),
      db.query(countQuery)
    ]);

    return {
      data: dataResult.rows,
      total: parseInt(countResult.rows[0].count),
      page,
      limit,
      totalPages: Math.ceil(countResult.rows[0].count / limit)
    };
  }

  async findById(id, client) {
    const db = client || this.db;
    const query = format('SELECT * FROM %I WHERE %I = $1', this.tableName, this.idColumn);
    const result = await db.query(query, [id]);
    return result.rows[0];
  }

  async create(data, client) {
    const db = client || this.db;
    const keys = Object.keys(data);
    const values = Object.values(data);

    const columns = keys.map((key) => format('%I', key)).join(', ');
    const placeholders = keys.map((_, i) => `$${i + 1}`).join(', ');

    const query = format(
      'INSERT INTO %I (%s) VALUES (%s) RETURNING *',
      this.tableName,
      columns,
      placeholders
    );

    const result = await db.query(query, values);
    return result.rows[0];
  }

  async update(id, data, client) {
    const db = client || this.db;
    const keys = Object.keys(data);
    const values = Object.values(data);

    const setClause = keys
      .map((key, i) => format('%I = $%s', key, i + 1))
      .join(', ');

    const query = format(
      'UPDATE %I SET %s WHERE %I = $%s RETURNING *',
      this.tableName,
      setClause,
      this.idColumn,
      keys.length + 1
    );

    const result = await db.query(query, [...values, id]);
    return result.rows[0];
  }

  async delete(id, client) {
    const db = client || this.db;
    const query = format(
      'DELETE FROM %I WHERE %I = $1 RETURNING *',
      this.tableName,
      this.idColumn
    );

    const result = await db.query(query, [id]);

    return result.rowCount > 0;
  }
}
module.exports = BaseRepository;
