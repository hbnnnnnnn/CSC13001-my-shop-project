// product service to handle business logic related to products

const productRepository = require("../repositories/product.repository.js");
const {
  indexProduct,
  indexUpdateProduct,
  indexDeleteProduct,
} = require("./search.service.js");
const cacheService = require("../utils/cache.util.js");

const getProducts = async ({
  page = 1,
  limit = 10,
  filter = {},
  sort = {},
} = {}, client) => {
  try {
    const cacheKey = `products:p:${page}:l:${limit}:f:${JSON.stringify(filter)}:s:${JSON.stringify(sort)}`;
    const cachedProducts = await cacheService.get(cacheKey);
    if (cachedProducts) {
      console.log(`[Cache Hit] ${cacheKey}`);
      return cachedProducts;
    }

    const products = await productRepository.findAllFiltered({
      page,
      limit,
      filter,
      sort,
    }, client);

    await cacheService.set(cacheKey, products, 300); // Cache for 5 minutes
    return products;
  } catch (error) {
    throw error;
  }
};

const getProductById = async (id, client) => {
  try {
    const cacheKey = `product:${id}`;
    const cachedProduct = await cacheService.get(cacheKey);
    if (cachedProduct) {
      console.log(`[Cache Hit] ${cacheKey}`);
      return cachedProduct;
    }

    const product = await productRepository.findById(id, client);
    if (product) {
      await cacheService.set(cacheKey, product, 900); // Cache for 15 mins
    }
    return product;
  } catch (error) {
    throw error;
  }
};

// dùng để tìm ids để update stock trong order service, dùng lệnh FOR UPDATE để khóa các dòng
const getProductsByIdsForUpdate = async (ids, client) => {
  try {
    // Không dùng Cache khi gọi For Update để bảo đảm dữ liệu RAM là mới nhất từ DB
    const products = await productRepository.findByIdsForUpdate(ids, client);
    return products;
  } catch (error) {
    throw error;
  }
};

const createProduct = async (product, client) => {
  try {
    const newProduct = await productRepository.create(product, client);
    const productWithCategory = await productRepository.findByIdWithCategory(
      newProduct.product_id,
      client
    );
    await indexProduct(productWithCategory);

    // Invalidate product lists and dashboard stats
    await cacheService.delByPrefix("products:p:");
    await cacheService.delByPrefix("products:low_stock:");

    return newProduct;
  } catch (error) {
    throw error;
  }
};

const updateProduct = async (id, product, client) => {
  try {
    const updatedProduct = await productRepository.update(id, product, client);
    const productWithCategory = await productRepository.findByIdWithCategory(
      updatedProduct.product_id,
      client
    );
    await indexUpdateProduct(id, productWithCategory);

    // Invalidate relevant caches
    await cacheService.del(`product:${id}`);
    await cacheService.delByPrefix("products:p:");
    if (product.stock !== undefined) await cacheService.delByPrefix("products:low_stock:");
    if (product.price !== undefined) await cacheService.delByPrefix("products:top_selling:");

    return updatedProduct;
  } catch (error) {
    throw error;
  }
};

const deleteProduct = async (id, client) => {
  try {
    const deletedProduct = await productRepository.delete(id, client);
    await indexDeleteProduct(id);

    // Invalidate caches
    await cacheService.del(`product:${id}`);
    await cacheService.delByPrefix("products:p:");
    await cacheService.delByPrefix("products:low_stock:");
    await cacheService.delByPrefix("products:top_selling:");

    return deletedProduct;
  } catch (error) {
    throw error;
  }
};

const updateProductStock = async (productId, newStock, client) => {
  try {
    const updatedProduct = await productRepository.updateStock(productId, newStock, client);
    const productWithCategory = await productRepository.findByIdWithCategory(
      productId,
      client
    );
    // Note: If calling from a transaction, ES sync should be handled by the caller after commit
    // But for standalone calls, we can sync here. 
    // We'll return the full product data so the caller can choose to sync.

    // Invalidate caches
    await cacheService.del(`product:${productId}`);
    await cacheService.delByPrefix("products:p:");
    await cacheService.delByPrefix("products:low_stock:");

    return productWithCategory;
  } catch (error) {
    throw error;
  }
};

const getTopLowStockProducts = async (limit = 5, client) => {
  try {
    const cacheKey = `products:low_stock:l:${limit}`;
    const cachedProducts = await cacheService.get(cacheKey);
    if (cachedProducts) {
      console.log(`[Cache Hit] ${cacheKey}`);
      return cachedProducts;
    }

    const products = await productRepository.findTopLowStockProducts(limit, client);
    await cacheService.set(cacheKey, products, 900); // Cache for 15 mins
    return products;
  } catch (error) {
    throw error;
  }
};

const getTopSellingProducts = async (limit = 5, client) => {
  try {
    const cacheKey = `products:top_selling:l:${limit}`;
    const cachedProducts = await cacheService.get(cacheKey);
    if (cachedProducts) {
      console.log(`[Cache Hit] ${cacheKey}`);
      return cachedProducts;
    }

    const products = await productRepository.findTopSellingProducts(limit, client);
    await cacheService.set(cacheKey, products, 3600); // Stats change slowly, cache for 1 hr
    return products;
  } catch (error) {
    throw error;
  }
};

module.exports = {
  getProducts,
  getProductById,
  getProductsByIdsForUpdate,
  createProduct,
  updateProduct,
  deleteProduct,
  updateProductStock,
  getTopLowStockProducts,
  getTopSellingProducts,
};

