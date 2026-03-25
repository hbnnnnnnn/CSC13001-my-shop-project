const orderService = require('../../services/order.service.js');
const { requireRole } = require('../../middlewares/auth.middleware.js');

const orderResolver = {
    Query: {
        orders: requireRole(['Admin', 'Sale'], async (_, { page, limit }) => {
            return await orderService.getAllOrders({ page, limit });
        }),
        order: requireRole(['Admin', 'Sale'], async (_, { id }) => {
            return await orderService.getOrderById(id);
        })
    },
    Mutation: {
        createOrder: requireRole(['Admin', 'Sale'], async (_, { customer_id, account_id, shipping_address, items }) => {
            return await orderService.createOrder({ customer_id, account_id, shipping_address }, items);
        }),
        updateOrderStatus: requireRole(['Admin', 'Sale'], async (_, { id, status }) => {
            return await orderService.updateOrderStatus(id, status);
        })
    }
};

module.exports = orderResolver;
