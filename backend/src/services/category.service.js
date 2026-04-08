const categoryRepository = require('../repositories/category.repository.js');
const cacheService = require('../utils/cache.util.js');

const getAllCategories = async (page = 1, limit = 10) => {
    const cacheKey = `categories:all:p:${page}:l:${limit}`;
    const cachedCategories = await cacheService.get(cacheKey);
    if (cachedCategories) {
        console.log(`[Cache Hit] ${cacheKey}`);
        return cachedCategories;
    }

    const categories = await categoryRepository.findAll({ page, limit });
    await cacheService.set(cacheKey, categories, 600); // 10 minutes
    return categories;
};

const getCategoryById = async (id) => {
    const cacheKey = `category:${id}`;
    const cachedCategory = await cacheService.get(cacheKey);
    if (cachedCategory) {
        console.log(`[Cache Hit] ${cacheKey}`);
        return cachedCategory;
    }

    const category = await categoryRepository.findById(id);
    if (!category) {
        throw new Error('Category not found');
    }
    
    await cacheService.set(cacheKey, category, 1800); // 30 mins
    return category;
};

const createCategory = async (name, description) => {
    const existing = await categoryRepository.findByName(name);
    if (existing) {
        throw new Error('Category name already exists');
    }

    const newCategory = await categoryRepository.create({
        name,
        description
    });

    // Invalidate pagination cache
    await cacheService.delByPrefix("categories:all:p:");
    return newCategory;
};

const updateCategory = async (id, name, description) => {
    const existing = await categoryRepository.findById(id);
    if (!existing) {
        throw new Error('Category not found');
    }

    if (name && name !== existing.name) {
        const nameExists = await categoryRepository.findByName(name);
        if (nameExists) {
            throw new Error('Category name already exists');
        }
    }

    const dataToUpdate = {};
    if (name !== undefined) dataToUpdate.name = name;
    if (description !== undefined) dataToUpdate.description = description;

    const updatedCategory = await categoryRepository.update(id, dataToUpdate);

    // Invalidate caches
    await cacheService.del(`category:${id}`);
    await cacheService.delByPrefix("categories:all:p:");

    return updatedCategory;
};

const deleteCategory = async (id) => {
    const existing = await categoryRepository.findById(id);
    if (!existing) {
        throw new Error('Category not found');
    }

    const deletedCategory = await categoryRepository.delete(id);

    // Invalidate caches
    await cacheService.del(`category:${id}`);
    await cacheService.delByPrefix("categories:all:p:");

    return deletedCategory;
};

module.exports = {
    getAllCategories,
    getCategoryById,
    createCategory,
    updateCategory,
    deleteCategory
};
