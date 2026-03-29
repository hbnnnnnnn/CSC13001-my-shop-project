const {
  getProducts,
  getProductById,
  getTopLowStockProducts,
  getTopSellingProducts,
  createProduct,
  updateProduct,
  deleteProduct,
} = require("../../services/product.service");

const { searchProducts } = require("../../services/search.service");

const { getCategoryById } = require("../../services/category.service");
const { generateProductDetailsFromImage } = require("../../services/aiService");

const productResolver = {
  Query: {
    products: async (_, { page, limit, filter, sort }) => {
      return await getProducts({ page, limit, filter, sort });
    },
    product: async (_, { id }) => {
      return await getProductById(id);
    },
    topLowStockProducts: async (_, { limit }) => {
      return await getTopLowStockProducts(limit);
    },
    topSellingProducts: async (_, { limit }) => {
      return await getTopSellingProducts(limit);
    },
    productSearch: async (_, { query, page, limit, filter, sort }) => {
      return await searchProducts(query, page, limit, filter, sort);
    },
  },
  Mutation: {
    createProduct: async (_, { input }) => {
      return await createProduct(input);
    },
    updateProduct: async (_, { id, input }) => {
      return await updateProduct(id, input);
    },
    deleteProduct: async (_, { id }) => {
      return await deleteProduct(id);
    },
    generateProductDetailsFromImage: async (_, { image }) => {
      return await generateProductDetailsFromImage(image);
    },
  },
  Product: {
    category: async (parent) => {
      return await getCategoryById(parent.category_id);
    },
  },
};

module.exports = productResolver;

