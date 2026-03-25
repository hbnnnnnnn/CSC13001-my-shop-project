// product service to handle business logic related to products

const productRepository = require("../repositories/product.repository.js");
const {
  indexProduct,
  indexUpdateProduct,
  indexDeleteProduct,
} = require("./search.service.js");

const getProducts = async ({
  page = 1,
  limit = 10,
  filter = {},
  sort = {},
} = {}, client) => {
  try {
    const products = await productRepository.findAllFiltered({
      page,
      limit,
      filter,
      sort,
    }, client);
    return products;
  } catch (error) {
    throw error;
  }
};

const getProductById = async (id, client) => {
  try {
    const product = await productRepository.findById(id, client);
    return product;
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

    return updatedProduct;
  } catch (error) {
    throw error;
  }
};

const deleteProduct = async (id, client) => {
  try {
    const deletedProduct = await productRepository.delete(id, client);
    await indexDeleteProduct(id);

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
    return productWithCategory;
  } catch (error) {
    throw error;
  }
};

const getTopLowStockProducts = async (limit = 5, client) => {
  try {
    const products = await productRepository.findTopLowStockProducts(limit, client);
    return products;
  } catch (error) {
    throw error;
  }
};

const getTopSellingProducts = async (limit = 5, client) => {
  try {
    const products = await productRepository.findTopSellingProducts(limit, client);
    return products;
  } catch (error) {
    throw error;
  }
};

module.exports = {
  getProducts,
  getProductById,
  createProduct,
  updateProduct,
  deleteProduct,
  updateProductStock,
  getTopLowStockProducts,
  getTopSellingProducts,
};

