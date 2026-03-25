const { pool } = require('../config/db.js');
const orderRepository = require('../repositories/order.repository.js');
const orderItemRepository = require('../repositories/order_item.repository.js');
const productService = require('./product.service.js');
const { indexUpdateProduct } = require('./search.service.js');

const createOrder = async (orderData, items) => {
    const client = await pool.connect();
    const productsToSync = [];

    try {
        await client.query('BEGIN');

        let finalPrice = 0;
        const orderItemsToCreate = [];

        // 1. Validate stocks and calculate prices
        for (const item of items) {
            const product = await productService.getProductById(item.product_id, client);
            if (!product) {
                throw new Error(`Product with ID ${item.product_id} not found`);
            }

            if (product.stock < item.quantity) {
                throw new Error(`Insufficient stock for product: ${product.name}`);
            }

            const itemTotalPrice = product.price * item.quantity;
            finalPrice += itemTotalPrice;

            orderItemsToCreate.push({
                product_id: item.product_id,
                quantity: item.quantity,
                unit_sale_price: product.price,
                total_price: itemTotalPrice
            });

            // 2. Decrement stock
            const updatedProductWithCategory = await productService.updateProductStock(product.product_id, product.stock - item.quantity, client);
            productsToSync.push(updatedProductWithCategory);
        }

        // 3. Create Order
        const newOrder = await orderRepository.create({
            ...orderData,
            final_price: finalPrice,
            status: 'Created'
        }, client);

        // 4. Create Order Items
        for (const itemToCreate of orderItemsToCreate) {
            await orderItemRepository.create({
                ...itemToCreate,
                order_id: newOrder.order_id
            }, client);
        }

        await client.query('COMMIT');

        // 5. Sync to Elasticsearch AFTER commit
        for (const product of productsToSync) {
            await indexUpdateProduct(product.product_id, product);
        }

        // Return order with items
        return {
            ...newOrder,
            items: orderItemsToCreate
        };
    } catch (error) {
        await client.query('ROLLBACK');
        throw error;
    } finally {
        client.release();
    }
};

const getAllOrders = async ({ page = 1, limit = 10 } = {}) => {
    return await orderRepository.findAll({ page, limit });
};

const getOrderById = async (id) => {
    const order = await orderRepository.findById(id);
    if (!order) {
        throw new Error('Order not found');
    }

    const items = await orderItemRepository.findByOrderId(id);
    return {
        ...order,
        items
    };
};

const updateOrderStatus = async (id, status) => {
    const client = await pool.connect();
    const productsToSync = [];

    try {
        await client.query('BEGIN');

        const existing = await orderRepository.findById(id, client);
        if (!existing) {
            throw new Error('Order not found');
        }

        // Logic for cancelling order and restoring stock
        if (status === 'Cancelled' && existing.status !== 'Cancelled') {
            const items = await orderItemRepository.findByOrderId(id, client);
            for (const item of items) {
                const product = await productService.getProductById(item.product_id, client);
                if (product) {
                    const updatedProductWithCategory = await productService.updateProductStock(product.product_id, product.stock + item.quantity, client);
                    productsToSync.push(updatedProductWithCategory);
                }
            }
        }

        const updatedOrder = await orderRepository.update(id, { status }, client);

        await client.query('COMMIT');

        // Sync to Elasticsearch AFTER commit
        for (const product of productsToSync) {
            await indexUpdateProduct(product.product_id, product);
        }

        return updatedOrder;
    } catch (error) {
        await client.query('ROLLBACK');
        throw error;
    } finally {
        client.release();
    }
};

module.exports = {
    createOrder,
    getAllOrders,
    getOrderById,
    updateOrderStatus
};
