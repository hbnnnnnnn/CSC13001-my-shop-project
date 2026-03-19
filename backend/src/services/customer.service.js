const customerRepository = require('../repositories/customer.repository.js');

const getAllCustomers = async (page = 1, limit = 10) => {
    return await customerRepository.findAll({ page, limit });
};

const getCustomerById = async (id) => {
    const customer = await customerRepository.findById(id);
    if (!customer) {
        throw new Error('Customer not found');
    }
    return customer;
};

const createCustomer = async (name, phone, address) => {
    if (phone) {
        const existing = await customerRepository.findByPhone(phone);
        if (existing) {
            throw new Error('Customer phone already exists');
        }
    }

    return await customerRepository.create({
        name,
        phone,
        address
    });
};

const updateCustomer = async (id, name, phone, address) => {
    const existing = await customerRepository.findById(id);
    if (!existing) {
        throw new Error('Customer not found');
    }

    if (phone && phone !== existing.phone) {
        const phoneExists = await customerRepository.findByPhone(phone);
        if (phoneExists) {
            throw new Error('Customer phone already exists');
        }
    }

    const dataToUpdate = {};
    if (name !== undefined) dataToUpdate.name = name;
    if (phone !== undefined) dataToUpdate.phone = phone;
    if (address !== undefined) dataToUpdate.address = address;

    return await customerRepository.update(id, dataToUpdate);
};

const deleteCustomer = async (id) => {
    const existing = await customerRepository.findById(id);
    if (!existing) {
        throw new Error('Customer not found');
    }

    return await customerRepository.delete(id);
};

module.exports = {
    getAllCustomers,
    getCustomerById,
    createCustomer,
    updateCustomer,
    deleteCustomer
};
