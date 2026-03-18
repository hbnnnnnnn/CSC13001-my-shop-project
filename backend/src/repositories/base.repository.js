// chú ý sql injection, dùng $1, $2, ... và pg-format để tránh
const format = require('pg-format');

class BaseRepository {
  constructor(tableName, idColumn, db) {
    this.tableName = tableName;
    this.idColumn = idColumn;
    this.db = db;
  }

  async findAll() {
    const query = format('SELECT * FROM %I', this.tableName);
    const result = await this.db.query(query);
    return result.rows;
  }

  async findById(id) {
    const query = format('SELECT * FROM %I WHERE %I = $1', this.tableName, this.idColumn);
    const result = await this.db.query(query, [id]);
    return result.rows[0];
  }

  async create(data) {
    const keys = Object.keys(data);
    const values = Object.values(data);

    const columns = keys.map((key) => format('%I', key)).join(', ');
    const placeholders = keys.map((_, i) => `$${i + 1}`).join(', ');

    // %I escapes array of column names safely (e.g. "col1", "col2")
    // %L escapes array of values as literals (e.g. 'val1', 2, 'val3')
    const query = format(
      'INSERT INTO %I (%s) VALUES (%s) RETURNING *',
      this.tableName,
      columns,
      placeholders
    );

    const result = await this.db.query(query, values);
    return result.rows[0];
  }

  async update(id, data) {
    const keys = Object.keys(data);
    const values = Object.values(data);

    // "name" = $1, "price" = $2
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

    const result = await this.db.query(query, [...values, id]);
    return result.rows[0];
  }

  async delete(id) {
    const query = format(
      'DELETE FROM %I WHERE %I = $1 RETURNING *',
      this.tableName,
      this.idColumn
    );

    const result = await this.db.query(query, [id]);

    return result.rowCount > 0;
  }
}
module.exports = BaseRepository;
