const { pool } = require('../config/db.js');
const orderRepository = require('../repositories/order.repository.js');
const orderItemRepository = require('../repositories/order_item.repository.js');
const productService = require('./product.service.js');
const { indexUpdateProduct } = require('./search.service.js');
const cacheService = require("../utils/cache.util.js");

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

        // Sync to Elasticsearch AFTER commit
        for (const product of productsToSync) {
            await indexUpdateProduct(product.product_id, product);
        }

        // Invalidate orders list cache
        await cacheService.delByPrefix("orders:all:p:");

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
    const cacheKey = `orders:all:p:${page}:l:${limit}`;
    const cachedOrders = await cacheService.get(cacheKey);
    if (cachedOrders) {
        console.log(`[Cache Hit] ${cacheKey}`);
        return cachedOrders;
    }

    const orders = await orderRepository.findAll({ page, limit });
    await cacheService.set(cacheKey, orders, 300); // Cache for 5 mins
    return orders;
};

const getOrderById = async (id) => {
    const cacheKey = `order:${id}`;
    const cachedOrder = await cacheService.get(cacheKey);
    if (cachedOrder) {
        console.log(`[Cache Hit] ${cacheKey}`);
        return cachedOrder;
    }

    const order = await orderRepository.findById(id);
    if (!order) {
        throw new Error('Order not found');
    }

    const items = await orderItemRepository.findByOrderId(id);
    const orderDetails = {
        ...order,
        items
    };

    // Orders are mostly immutable, we can cache them for a long time
    await cacheService.set(cacheKey, orderDetails, 3600); // 1 hour

    return orderDetails;
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

        // Invalidate specific order and list caches
        await cacheService.del(`order:${id}`);
        await cacheService.delByPrefix("orders:all:p:");

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
