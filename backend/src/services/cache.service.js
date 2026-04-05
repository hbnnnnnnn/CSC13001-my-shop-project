const { redisClient } = require('../config/redis');

/**
 * Wrapper for common cache operations
 */
const cacheService = {
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
      // Using SCAN is safer than KEYS * for performance
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

module.exports = cacheService;
