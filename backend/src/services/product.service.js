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
} = {}) => {
  try {
    const products = await productRepository.findAllFiltered({
      page,
      limit,
      filter,
      sort,
    });
    return products;
  } catch (error) {
    throw error;
  }
};

const getProductById = async (id) => {
  try {
    const product = await productRepository.findById(id);
    return product;
  } catch (error) {
    throw error;
  }
};

const createProduct = async (product) => {
  try {
    const newProduct = await productRepository.create(product);
    const productWithCategory = await productRepository.findByIdWithCategory(
      newProduct.product_id,
    );
    await indexProduct(productWithCategory);

    return newProduct;
  } catch (error) {
    throw error;
  }
};

const updateProduct = async (id, product) => {
  try {
    const updatedProduct = await productRepository.update(id, product);
    const productWithCategory = await productRepository.findByIdWithCategory(
      updatedProduct.product_id,
    );
    await indexUpdateProduct(id, productWithCategory);

    return updatedProduct;
  } catch (error) {
    throw error;
  }
};

const deleteProduct = async (id) => {
  try {
    const deletedProduct = await productRepository.delete(id);
    await indexDeleteProduct(id);

    return deletedProduct;
  } catch (error) {
    throw error;
  }
};

const getTopLowStockProducts = async (limit = 5) => {
  try {
    const products = await productRepository.findTopLowStockProducts(limit);
    return products;
  } catch (error) {
    throw error;
  }
};

const getTopSellingProducts = async (limit = 5) => {
  try {
    const products = await productRepository.findTopSellingProducts(limit);
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
  getTopLowStockProducts,
  getTopSellingProducts,
};

