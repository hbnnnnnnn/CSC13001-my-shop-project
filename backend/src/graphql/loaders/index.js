const DataLoader = require('dataloader');
const customerRepository = require('../../repositories/customer.repository');
const accountRepository = require('../../repositories/account.repository');
const productRepository = require('../../repositories/product.repository');
const categoryRepository = require('../../repositories/category.repository');
const orderItemRepository = require('../../repositories/order_item.repository');
const cacheUtil = require('../../utils/cache.util');

/**
 * Hàm trợ giúp để thực hiện fetch hàng loạt có tích hợp Redis Cache.
 * Luồng: Check Redis (mget) -> Lấy ID thiếu từ DB -> Lưu lại vào Redis (mset) -> Trả về kết quả khớp thứ tự.
 */
const batchFetchWithCache = async (ids, { repository, cacheKeyPrefix, idField, ttlSeconds = 3600 }) => {
    const keys = ids.map(id => `${cacheKeyPrefix}:${id}`);
    
    // 1. Kiểm tra cache Redis tập trung (MGET)
    const cachedValues = await cacheUtil.getMany(keys);
    
    const results = new Array(ids.length);
    const missingIds = [];
    const missingIndices = [];
    let hits = 0;

    cachedValues.forEach((val, i) => {
        if (val) {
            results[i] = val;
            hits++;
        } else {
            missingIds.push(ids[i]);
            missingIndices.push(i);
        }
    });

    // Log tổng hợp để theo dõi hiệu năng
    if (ids.length > 0) {
        const misses = ids.length - hits;
        console.log(`[DataLoader Cache] ${cacheKeyPrefix}: ${hits} hits, ${misses} misses`);
    }

    // 2. Nếu có cache miss, truy vấn Database
    if (missingIds.length > 0) {
        const dbResults = await repository.findByIds(missingIds);
        const dbMap = dbResults.reduce((acc, item) => {
            acc[item[idField]] = item;
            return acc;
        }, {});

        const entriesToCache = {};
        missingIds.forEach((id, i) => {
            const data = dbMap[id] || null;
            const originalIndex = missingIndices[i];
            results[originalIndex] = data;
            
            // Chỉ cache những bản ghi tồn tại
            if (data) {
                entriesToCache[`${cacheKeyPrefix}:${id}`] = data;
            }
        });

        // 3. Cập nhật ngược lại vào Redis hàng loạt (Pipeline/MSET)
        if (Object.keys(entriesToCache).length > 0) {
            await cacheUtil.setMany(entriesToCache, ttlSeconds);
        }
    }

    return results;
};

/**
 * Tạo các instance DataLoader cho mỗi request.
 */
const createLoaders = () => {
    return {
        // Loader cho Customer (Cache 30p)
        customer: new DataLoader(async (ids) => {
            return await batchFetchWithCache(ids, {
                repository: customerRepository,
                cacheKeyPrefix: 'customer',
                idField: 'customer_id',
                ttlSeconds: 1800
            });
        }),

        // Loader cho Account (Cache 10p cho bảo mật)
        account: new DataLoader(async (ids) => {
            return await batchFetchWithCache(ids, {
                repository: accountRepository,
                cacheKeyPrefix: 'account',
                idField: 'account_id',
                ttlSeconds: 600
            });
        }),

        // Loader cho Product (Cache 1h)
        product: new DataLoader(async (ids) => {
            return await batchFetchWithCache(ids, {
                repository: productRepository,
                cacheKeyPrefix: 'product',
                idField: 'product_id',
                ttlSeconds: 3600
            });
        }),

        // Loader cho Category (Cache 1h)
        category: new DataLoader(async (ids) => {
            return await batchFetchWithCache(ids, {
                repository: categoryRepository,
                cacheKeyPrefix: 'category',
                idField: 'category_id',
                ttlSeconds: 3600
            });
        }),

        // Loader cho OrderItems (Một-Nhiều)
        orderItems: new DataLoader(async (orderIds) => {
            const allItems = await orderItemRepository.findByOrderIds(orderIds);
            const itemsGroupedByOrderId = allItems.reduce((acc, item) => {
                if (!acc[item.order_id]) acc[item.order_id] = [];
                acc[item.order_id].push(item);
                return acc;
            }, {});
            return orderIds.map(id => itemsGroupedByOrderId[id] || []);
        })
    };
};

module.exports = { createLoaders };
