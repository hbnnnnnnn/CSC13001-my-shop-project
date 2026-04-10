const customerRepository = require('../repositories/customer.repository.js');
const cacheService = require('../utils/cache.util.js');

const getAllCustomers = async (page = 1, limit = 10) => {
    const cacheKey = `customers:all:p:${page}:l:${limit}`;
    const cachedCustomers = await cacheService.get(cacheKey);
    if (cachedCustomers) {
        console.log(`[Cache Hit] ${cacheKey}`);
        return cachedCustomers;
    }

    const customers = await customerRepository.findAll({ page, limit });
    await cacheService.set(cacheKey, customers, 600); // 10 minutes
    return customers;
};

const getCustomerById = async (id) => {
    const cacheKey = `customer:${id}`;
    const cachedCustomer = await cacheService.get(cacheKey);
    if (cachedCustomer) {
        console.log(`[Cache Hit] ${cacheKey}`);
        return cachedCustomer;
    }

    const customer = await customerRepository.findById(id);
    if (!customer) {
        throw new Error('Customer not found');
    }

    await cacheService.set(cacheKey, customer, 1800); // 30 mins
    return customer;
};

const getCustomerByPhone = async (phone) => {
    const cacheKey = `customer:phone:${phone}`;
    const cachedCustomer = await cacheService.get(cacheKey);
    if (cachedCustomer) {
        console.log(`[Cache Hit] ${cacheKey}`);
        return cachedCustomer;
    }

    const customer = await customerRepository.findByPhone(phone);
    if (!customer) {
        throw new Error('Customer not found');
    }

    await cacheService.set(cacheKey, customer, 1800); // 30 mins
    return customer;
};

const getCustomerByEmail = async (email) => {
    const cacheKey = `customer:email:${email}`;
    const cachedCustomer = await cacheService.get(cacheKey);
    if (cachedCustomer) {
        console.log(`[Cache Hit] ${cacheKey}`);
        return cachedCustomer;
    }

    const customer = await customerRepository.findByEmail(email);

    if (!customer) {
        throw new Error('Customer with this email not found');
    }

    await cacheService.set(cacheKey, customer, 1800); // 30 mins
    return customer;
};

const validateEmail = (email) => {
    const re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return re.test(String(email).toLowerCase());
};

const createCustomer = async (name, phone, address, email) => {
    if (phone) {
        const existing = await customerRepository.findByPhone(phone);
        if (existing) {
            throw new Error('Customer phone already exists');
        }
    }

    if (email) {
        if (!validateEmail(email)) {
            throw new Error('Invalid email format');
        }
        // Check unique email
        const existingEmail = await customerRepository.findByEmail(email);
        if (existingEmail) {
            throw new Error('Customer email already exists');
        }
    }

    const newCustomer = await customerRepository.create({
        name,
        phone,
        address,
        email
    });

    // Invalidate pagination cache
    await cacheService.delByPrefix("customers:all:p:");
    return newCustomer;
};

const updateCustomer = async (id, name, phone, address, email) => {
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

    if (email && email !== existing.email) {
        if (!validateEmail(email)) {
            throw new Error('Invalid email format');
        }
        const emailExists = await customerRepository.findByEmail(email);
        if (emailExists) {
            throw new Error('Customer email already exists');
        }
    }

    const dataToUpdate = {};
    if (name !== undefined) dataToUpdate.name = name;
    if (phone !== undefined) dataToUpdate.phone = phone;
    if (address !== undefined) dataToUpdate.address = address;
    if (email !== undefined) dataToUpdate.email = email;

    const updatedCustomer = await customerRepository.update(id, dataToUpdate);

    // Invalidate caches
    await cacheService.del(`customer:${id}`);
    if (existing.phone) await cacheService.del(`customer:phone:${existing.phone}`);
    if (phone) await cacheService.del(`customer:phone:${phone}`);
    if (existing.email) await cacheService.del(`customer:email:${existing.email}`);
    if (email) await cacheService.del(`customer:email:${email}`);
    await cacheService.delByPrefix("customers:all:p:");

    return updatedCustomer;
};

const deleteCustomer = async (id) => {
    const existing = await customerRepository.findById(id);
    if (!existing) {
        throw new Error('Customer not found');
    }

    const deletedCustomer = await customerRepository.delete(id);

    // Invalidate caches
    await cacheService.del(`customer:${id}`);
    if (existing.phone) await cacheService.del(`customer:phone:${existing.phone}`);
    if (existing.email) await cacheService.del(`customer:email:${existing.email}`);
    await cacheService.delByPrefix("customers:all:p:");

    return deletedCustomer;
};

module.exports = {
    getAllCustomers,
    getCustomerById,
    getCustomerByPhone,
    getCustomerByEmail,
    createCustomer,
    updateCustomer,
    deleteCustomer
};
