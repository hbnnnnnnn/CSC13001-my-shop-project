const customerService = require('../../services/customer.service.js');
const { requireRole } = require('../../middlewares/auth.middleware.js');

const customerResolver = {
    Query: {
        customers: async (_, { page, limit }) => {
            return await customerService.getAllCustomers(page, limit);
        },
        customer: async (_, { id }) => {
            return await customerService.getCustomerById(id);
        },
        customerByPhone: async (_, { phone }) => {
            return await customerService.getCustomerByPhone(phone);
        },
        customerByEmail: async (_, { email }) => {
            return await customerService.getCustomerByEmail(email);
        }
    },
    Mutation: {
        createCustomer: requireRole(['Admin'], async (_, { name, phone, email, address }) => {
            return await customerService.createCustomer(name, phone, address, email);
        }),
        updateCustomer: requireRole(['Admin'], async (_, { id, name, phone, email, address }) => {
            return await customerService.updateCustomer(id, name, phone, address, email);
        }),
        deleteCustomer: requireRole(['Admin'], async (_, { id }) => {
            return await customerService.deleteCustomer(id);
        })
    }
};

module.exports = customerResolver;
