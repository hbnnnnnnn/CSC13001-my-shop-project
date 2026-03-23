const categoryRepository = require('../repositories/category.repository.js');

const getAllCategories = async (page = 1, limit = 10) => {
    return await categoryRepository.findAll({ page, limit });
};

const getCategoryById = async (id) => {
    const category = await categoryRepository.findById(id);
    if (!category) {
        throw new Error('Category not found');
    }
    return category;
};

const createCategory = async (name, description) => {
    const existing = await categoryRepository.findByName(name);
    if (existing) {
        throw new Error('Category name already exists');
    }

    return await categoryRepository.create({
        name,
        description
    });
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

    return await categoryRepository.update(id, dataToUpdate);
};

const deleteCategory = async (id) => {
    const existing = await categoryRepository.findById(id);
    if (!existing) {
        throw new Error('Category not found');
    }

    return await categoryRepository.delete(id);
};

module.exports = {
    getAllCategories,
    getCategoryById,
    createCategory,
    updateCategory,
    deleteCategory
};
