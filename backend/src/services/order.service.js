const { pool } = require('../config/db.js');
const orderRepository = require('../repositories/order.repository.js');
const orderItemRepository = require('../repositories/order_item.repository.js');
const customerRepository = require('../repositories/customer.repository.js');
const productService = require('./product.service.js');
const { indexUpdateProduct } = require('./search.service.js');
const cacheService = require("../utils/cache.util.js");

const createOrder = async (orderData, items) => {
    const client = await pool.connect();
    const productsToSync = [];

    try {
        await client.query('BEGIN');

        // Bổ sung thông tin người nhận từ Customer nếu thiếu
        if (orderData.customer_id && (!orderData.recipient_name || !orderData.recipient_phone)) {
            const customer = await customerRepository.findById(orderData.customer_id, client);
            if (customer) {
                if (!orderData.recipient_name) orderData.recipient_name = customer.name;
                if (!orderData.recipient_phone) orderData.recipient_phone = customer.phone;
                if (!orderData.recipient_email) orderData.recipient_email = customer.email;
            }
        }

        let finalPrice = 0;
        const orderItemsToCreate = [];
        const normalizedItems = _normalizeItems(items);

        // 1. Validate stocks and calculate prices
        for (const item of normalizedItems) {
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

// --- INTERNAL HELPER FUNCTIONS ---

const _normalizeItems = (inputItems) => {
    if (!inputItems || inputItems.length === 0) {
        throw new Error('Order must have at least one item');
    }
    const itemMap = new Map();
    for (const item of inputItems) {
        const idStr = item.product_id.toString();

        if (!Number.isInteger(item.quantity) || item.quantity <= 0) {
            throw new Error(`Invalid quantity for product ${idStr}`);
        }

        if (itemMap.has(idStr)) {
            const existing = itemMap.get(idStr);
            existing.quantity += item.quantity;
        } else {
            itemMap.set(idStr, { ...item, product_id: idStr });
        }
    }
    return Array.from(itemMap.values());
};

const _validateTransition = (currentStatus, targetStatus) => {
    const allowedTransitions = {
        'Created': ['Processing', 'Cancelled'],
        'Processing': ['Shipped', 'Cancelled'],
        'Shipped': ['Delivered'],
        'Delivered': [],
        'Cancelled': []
    };

    if (currentStatus === targetStatus) return; // No change

    const allowed = allowedTransitions[currentStatus] || [];
    if (!allowed.includes(targetStatus)) {
        throw new Error(`Invalid status transition from ${currentStatus} to ${targetStatus}`);
    }
};

const _checkUpdatePermissions = (targetStatus, updateInput) => {
    if (targetStatus === 'Delivered' || targetStatus === 'Cancelled') {
        throw new Error(`Cannot edit an archived order (${targetStatus})`);
    }
    if (targetStatus === 'Shipped') {
        const hasInfoUpdate = ['shipping_address', 'recipient_name', 'recipient_phone', 'recipient_email'].some(key => updateInput[key] !== undefined);
        if (updateInput.items || hasInfoUpdate) {
            throw new Error(`Cannot update items or info for a shipped order`);
        }
    }
};

const _cancelOrderStock = async (oldItems, client) => {
    if (!oldItems || oldItems.length === 0) return [];
    const productIds = oldItems.map(item => item.product_id);
    const lockedProducts = await productService.getProductsByIdsForUpdate(productIds, client);

    // Virtual stock calculation
    const virtualStock = {};
    for (const p of lockedProducts) {
        virtualStock[p.product_id] = p.stock;
    }

    for (const item of oldItems) {
        if (virtualStock[item.product_id] === undefined) {
            throw new Error(`Product ${item.product_id} not found when restoring stock`);
        }
        virtualStock[item.product_id] += item.quantity;
    }

    // Mutate DB
    const syncList = [];
    for (const p of lockedProducts) {
        const newStock = virtualStock[p.product_id];
        const updatedProduct = await productService.updateProductStock(p.product_id, newStock, client);
        syncList.push(updatedProduct);
    }
    return syncList;
};

const _replaceOrderItems = async (orderId, targetStatus, oldItems, newItemsArray, client) => {
    // 1. Load Data (Locking)
    const oldIds = oldItems.map(i => i.product_id.toString());
    const newIds = newItemsArray.map(i => i.product_id.toString());
    const allIds = [...new Set([...oldIds, ...newIds])]; // Unique IDs

    const lockedProducts = await productService.getProductsByIdsForUpdate(allIds, client);

    if (lockedProducts.length !== allIds.length) {
        throw new Error('Some products not found');
    }

    const virtualMap = {}; // id -> { stock, price, productData }
    for (const p of lockedProducts) {
        virtualMap[p.product_id.toString()] = {
            stock: p.stock,
            price: p.price,
            productData: p
        };
    }

    // 2. Hoàn kho (Virtual Phase)
    for (const oldItem of oldItems) {
        const pIdStr = oldItem.product_id.toString();
        if (virtualMap[pIdStr]) {
            virtualMap[pIdStr].stock += oldItem.quantity;
        }
    }

    // 3. Trừ kho (Virtual Phase) & Tính Price
    let newFinalPrice = 0;
    const itemsToCreateData = [];

    for (const newItem of newItemsArray) {
        const pIdStr = newItem.product_id.toString();
        const productInfo = virtualMap[pIdStr];

        if (!productInfo) {
            throw new Error(`Product with ID ${newItem.product_id} not found`);
        }

        if (targetStatus !== 'Cancelled') {
            if (productInfo.stock < newItem.quantity) {
                throw new Error(`Insufficient stock for product ID: ${newItem.product_id}`);
            }
            productInfo.stock -= newItem.quantity;
        }

        const itemTotalPrice = productInfo.price * newItem.quantity;
        newFinalPrice += itemTotalPrice;

        itemsToCreateData.push({
            product_id: newItem.product_id,
            quantity: newItem.quantity,
            unit_sale_price: productInfo.price,
            total_price: itemTotalPrice
        });
    }

    // 4. Save Stock (Mutate)
    const syncList = [];
    for (const p of lockedProducts) {
        const pIdStr = p.product_id.toString();
        const newStock = virtualMap[pIdStr].stock;
        if (p.stock !== newStock) {
            const updatedCategoryProd = await productService.updateProductStock(p.product_id, newStock, client);
            syncList.push(updatedCategoryProd);
        }
    }

    // 5. Data Replacement (Mutate)
    await orderItemRepository.deleteByOrderId(orderId, client);
    for (const itemData of itemsToCreateData) {
        await orderItemRepository.create({
            ...itemData,
            order_id: orderId
        }, client);
    }

    return { newFinalPrice, syncList };
};

// --- MAIN EXPORTED FUNCTIONS ---

const updateOrderFull = async (id, input) => {
    const client = await pool.connect();
    try {
        await client.query('BEGIN');

        const existingOrder = await orderRepository.findById(id, client);
        if (!existingOrder) throw new Error('Order not found');
        if (existingOrder.is_deleted) throw new Error('Order is deleted');

        const targetStatus = input.status || existingOrder.status;

        _validateTransition(existingOrder.status, targetStatus);
        _checkUpdatePermissions(targetStatus, input);

        let productsToSync = [];
        let newFinalPrice = existingOrder.final_price;
        const oldItems = await orderItemRepository.findByOrderId(id, client);

        if (targetStatus === 'Cancelled') {
            // Cancelled flow: BỎ QUA input.items, hoàn kho cũ (nếu chưa hoàn)
            if (existingOrder.status !== 'Cancelled') {
                productsToSync = await _cancelOrderStock(oldItems, client);
            }
        } else if (input.items) {
            // Edit flow
            const normalizedItems = _normalizeItems(input.items);
            const replaceResult = await _replaceOrderItems(id, targetStatus, oldItems, normalizedItems, client);
            newFinalPrice = replaceResult.newFinalPrice;
            productsToSync = replaceResult.syncList;
        }

        // Cập nhật Update Data
        const dataToUpdate = {
            status: targetStatus,
            final_price: newFinalPrice // Keep final_price for historical record even if cancelled
        };
        const allowedInfo = ['shipping_address', 'recipient_name', 'recipient_phone', 'recipient_email'];
        for (const field of allowedInfo) {
            if (input[field] !== undefined) {
                dataToUpdate[field] = input[field];
            }
        }

        const updatedOrder = await orderRepository.update(id, dataToUpdate, client);

        await client.query('COMMIT');

        // Sync & Cache
        for (const product of productsToSync) {
            await indexUpdateProduct(product.product_id, product);
        }
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

const softDeleteOrder = async (id) => {
    const client = await pool.connect();
    try {
        await client.query('BEGIN');

        const existingOrder = await orderRepository.findById(id, client);
        if (!existingOrder) throw new Error('Order not found');
        if (existingOrder.is_deleted) throw new Error('Order already deleted');

        // Check if we are allowed to transition to Cancelled
        _validateTransition(existingOrder.status, 'Cancelled');

        let productsToSync = [];
        if (existingOrder.status !== 'Cancelled') {
            const oldItems = await orderItemRepository.findByOrderId(id, client);
            productsToSync = await _cancelOrderStock(oldItems, client);
        }

        await orderRepository.update(id, {
            status: 'Cancelled',
            is_deleted: true,
            deleted_at: new Date()
        }, client);

        await client.query('COMMIT');

        for (const product of productsToSync) {
            await indexUpdateProduct(product.product_id, product);
        }
        await cacheService.del(`order:${id}`);
        await cacheService.delByPrefix("orders:all:p:");

        return true;
    } catch (error) {
        await client.query('ROLLBACK');
        throw error;
    } finally {
        client.release();
    }
};

// Deprecated wraps for backward compatibility
const updateOrder = async (id, updateData) => {
    return await updateOrderFull(id, updateData);
};

const updateOrderStatus = async (id, status) => {
    return await updateOrderFull(id, { status });
};

module.exports = {
    createOrder,
    getAllOrders,
    getOrderById,
    updateOrderFull,
    softDeleteOrder,
    // Deprecated
    updateOrder,
    updateOrderStatus
};

