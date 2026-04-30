const esClient = require("../config/elasticsearch");
const cacheService = require("../utils/cache.util.js");
const INDEX = process.env.ELASTICSEARCH_PRODUCT_INDEX || "products";

// cost_price is internal-only; strip it from every document before sending to Elasticsearch
const _stripSensitiveFields = ({ cost_price, ...rest }) => rest;

const createIndex = async () => {
  if (
    await esClient.indices.exists({
      index: INDEX,
    })
  ) {
    return;
  }

  await esClient.indices.create({
    index: INDEX,
    body: {
      settings: {
        analysis: {
          analyzer: {
            product_analyzer: {
              type: "custom",
              tokenizer: "standard",
              filter: ["lowercase", "asciifolding"],
            },
          },
        },
      },
      mappings: {
        properties: {
          product_id: { type: "integer" },
          sku: { type: "text", analyzer: "product_analyzer" },
          name: {
            type: "text",
            analyzer: "product_analyzer",
            fields: {
              keyword: { type: "keyword" }, // raw value for sorting and filtering
            },
          },
          description: { type: "text", analyzer: "product_analyzer" },
          price: { type: "integer" },
          stock: { type: "integer" },
          supplier: { type: "text", analyzer: "product_analyzer" },
          category_id: { type: "integer" },
          category_name: { type: "text", analyzer: "product_analyzer" },
          created_time: { type: "date" },
          updated_time: { type: "date" },
        },
      },
    },
  });
};

const indexProduct = async (product) => {
  await esClient.index({
    index: INDEX,
    id: product.product_id,
    document: _stripSensitiveFields(product),
  });
  await cacheService.delByPrefix("search:");
};

const bulkIndexProducts = async (products) => {
  const operations = products.flatMap((product) => [
    {
      index: {
        _index: INDEX,
        _id: product.product_id,
      },
    },
    _stripSensitiveFields(product),
  ]);

  await esClient.bulk({
    body: operations,
  });
  await cacheService.delByPrefix("search:");
};

const indexUpdateProduct = async (productId, product) => {
  await esClient.update({
    index: INDEX,
    id: productId,
    doc: _stripSensitiveFields(product),
  });
  await cacheService.delByPrefix("search:");
};

const indexDeleteProduct = async (productId) => {
  await esClient.delete({
    index: INDEX,
    id: productId,
  });
  await cacheService.delByPrefix("search:");
};

const searchProducts = async (
  query,
  page = 1,
  limit = 10,
  filters = {},
  sort = {},
) => {
  const cacheKey = `search:q:${query}:p:${page}:l:${limit}:f:${JSON.stringify(filters)}:s:${JSON.stringify(sort)}`;
  const cachedResult = await cacheService.get(cacheKey);
  if (cachedResult) {
    console.log(`[Cache Hit] ${cacheKey}`);
    return cachedResult;
  }

  const filterClauses = [];

  if (filters.category_id) {
    filterClauses.push({
      term: {
        category_id: filters.category_id,
      },
    });
  }

  if (filters.min_price || filters.max_price) {
    filterClauses.push({
      range: {
        price: {
          ...(filters.min_price && { gte: filters.min_price }),
          ...(filters.max_price && { lte: filters.max_price }),
        },
      },
    });
  }

  // Map GraphQL sort enum to ES field names
  const SORT_FIELD_MAP = {
    PRICE: "price",
    NAME: "name.keyword",
    STOCK: "stock",
    CREATED_AT: "created_time",
    UPDATED_AT: "updated_time",
  };

  const sortClause = [];
  if (sort.field && sort.order) {
    sortClause.push({
      [SORT_FIELD_MAP[sort.field]]: { order: sort.order.toLowerCase() },
    });
  }

  const result = await esClient.search({
    index: INDEX,
    from: (page - 1) * limit,
    size: limit,
    sort: sortClause.length > 0 ? sortClause : undefined,
    query: {
      bool: {
        must: [
          {
            multi_match: {
              query,
              fields: [
                "sku",
                "name^2",
                "description",
                "supplier",
                "category_name",
              ],
              fuzziness: "AUTO",
            },
          },
        ],
        filter: filterClauses,
      },
    },
  });

  const searchResponse = {
    data: result.hits.hits.map((hit) => hit._source),
    total: result.hits.total.value,
    page,
    limit,
    totalPages: Math.ceil(result.hits.total.value / limit),
  };

  await cacheService.set(cacheKey, searchResponse, 300); // Cache for 5 mins
  return searchResponse;
};

module.exports = {
  createIndex,
  indexProduct,
  bulkIndexProducts,
  indexUpdateProduct,
  indexDeleteProduct,
  searchProducts,
};

