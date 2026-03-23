const customerService = require('../../services/customer.service.js');
const { requireRole } = require('../../middlewares/auth.middleware.js');

const customerResolver = {
    Query: {
        customers: async (_, { page, limit }) => {
            return await customerService.getAllCustomers(page, limit);
        },
        customer: async (_, { id }) => {
            return await customerService.getCustomerById(id);
        }
    },
    Mutation: {
        createCustomer: requireRole(['Admin'], async (_, { name, phone, address }) => {
            return await customerService.createCustomer(name, phone, address);
        }),
        updateCustomer: requireRole(['Admin'], async (_, { id, name, phone, address }) => {
            return await customerService.updateCustomer(id, name, phone, address);
        }),
        deleteCustomer: requireRole(['Admin'], async (_, { id }) => {
            return await customerService.deleteCustomer(id);
        })
    }
};

module.exports = customerResolver;
