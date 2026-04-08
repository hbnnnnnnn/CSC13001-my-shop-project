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
    },
    // Field resolvers
    Order: {
        customer: (parent, _, { loaders }) => {
            if (!parent.customer_id) return null;
            return loaders.customer.load(parent.customer_id);
        },
        account: (parent, _, { loaders }) => {
            if (!parent.account_id) return null;
            return loaders.account.load(parent.account_id);
        },
        items: (parent, _, { loaders }) => {
            // Tận dụng DataLoader để lấy tất cả items của nhiều orders trong 1 query duy nhất
            return loaders.orderItems.load(parent.order_id);
        }
    },
    OrderItem: {
        product: (parent, _, { loaders }) => {
            if (!parent.product_id) return null;
            return loaders.product.load(parent.product_id);
        }
    }
};

module.exports = orderResolver;
