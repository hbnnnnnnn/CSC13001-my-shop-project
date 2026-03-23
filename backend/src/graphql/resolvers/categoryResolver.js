const categoryService = require('../../services/category.service.js');
const { requireRole } = require('../../middlewares/auth.middleware.js');

const categoryResolver = {
    Query: {
        categories: async (_, { page, limit }) => {
            return await categoryService.getAllCategories(page, limit);
        },
        category: async (_, { id }) => {
            return await categoryService.getCategoryById(id);
        }
    },
    Mutation: {
        createCategory: requireRole(['Admin'], async (_, { name, description }) => {
            return await categoryService.createCategory(name, description);
        }),
        updateCategory: requireRole(['Admin'], async (_, { id, name, description }) => {
            return await categoryService.updateCategory(id, name, description);
        }),
        deleteCategory: requireRole(['Admin'], async (_, { id }) => {
            return await categoryService.deleteCategory(id);
        })
    }
};

module.exports = categoryResolver;
