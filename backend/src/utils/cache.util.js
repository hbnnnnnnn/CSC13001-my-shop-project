const { redisClient } = require('../config/redis');

/**
 * Wrapper for common cache operations
 * Moved to utils to avoid circular dependencies and serve as a technical utility.
 */
const cacheUtil = {
  /**
   * Get value from cache
   * @param {string} key 
   * @returns {any} parsed JSON or raw string
   */
  async get(key) {
    try {
      const data = await redisClient.get(key);
      if (!data) return null;
      try {
        return JSON.parse(data);
      } catch (e) {
        return data; // Not JSON
      }
    } catch (error) {
      console.error(`Redis Get Error [${key}]:`, error);
      return null;
    }
  },

  /**
   * Get multiple values from cache
   * @param {string[]} keys 
   * @returns {any[]} array of parsed values or nulls
   */
  async getMany(keys) {
    if (!keys || keys.length === 0) return [];
    try {
      const results = await redisClient.mGet(keys);
      return results.map(data => {
        if (!data) return null;
        try {
          return JSON.parse(data);
        } catch (e) {
          return data;
        }
      });
    } catch (error) {
      console.error(`Redis MGet Error:`, error);
      return keys.map(() => null);
    }
  },

  /**
   * Set value in cache with optional TTL
   * @param {string} key 
   * @param {any} value 
   * @param {number} ttlSeconds Time to live in seconds (default 1 hour)
   */
  async set(key, value, ttlSeconds = 3600) {
    try {
      const stringValue = typeof value === 'object' ? JSON.stringify(value) : String(value);
      await redisClient.set(key, stringValue, {
        EX: ttlSeconds,
      });
    } catch (error) {
      console.error(`Redis Set Error [${key}]:`, error);
    }
  },

  /**
   * Set multiple values in cache with TTL
   * @param {Object} entries Object with key-value pairs
   * @param {number} ttlSeconds 
   */
  async setMany(entries, ttlSeconds = 3600) {
    if (!entries || Object.keys(entries).length === 0) return;
    try {
      const pipeline = redisClient.multi();
      for (const [key, value] of Object.entries(entries)) {
        const stringValue = typeof value === 'object' ? JSON.stringify(value) : String(value);
        pipeline.set(key, stringValue, { EX: ttlSeconds });
      }
      await pipeline.exec();
    } catch (error) {
      console.error(`Redis MSet Error:`, error);
    }
  },

  /**
   * Delete specific key
   * @param {string} key 
   */
  async del(key) {
    try {
      await redisClient.del(key);
    } catch (error) {
      console.error(`Redis Del Error [${key}]:`, error);
    }
  },

  /**
   * Delete all keys matching a prefix
   * @param {string} prefix e.g., 'search:'
   */
  async delByPrefix(prefix) {
    try {
      let cursor = '0';
      const matchPattern = `${prefix}*`;
      do {
        const reply = await redisClient.scan(cursor, {
          MATCH: matchPattern,
          COUNT: 100
        });
        cursor = reply.cursor.toString();
        const keys = reply.keys;
        if (keys.length > 0) {
          await redisClient.del(keys);
        }
      } while (cursor !== '0');
    } catch (error) {
      console.error(`Redis DelByPrefix Error [${prefix}]:`, error);
    }
  }
};

module.exports = cacheUtil;
