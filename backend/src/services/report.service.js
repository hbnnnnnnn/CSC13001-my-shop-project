const reportRepository = require('../repositories/report.repository.js');
const cacheService = require('../utils/cache.util.js');

const getProductSalesReport = async ({ period = 'day', startDate = null, endDate = null } = {}) => {
    const cacheKey = `report:products:${period}:${startDate || 'all'}:${endDate || 'all'}`;

    const cached = await cacheService.get(cacheKey);
    if (cached) {
        console.log(`[Cache Hit] ${cacheKey}`);
        return cached;
    }

    const data = await reportRepository.getProductSalesReport(period, startDate, endDate);
    const groupedData = {};
    data.forEach(row => {
        if (!groupedData[row.period]) {
            groupedData[row.period] = {
                period: row.period,
                date: row.date,
                products: [],
                totalQuantity: 0,
                totalRevenue: 0
            };
        }
        groupedData[row.period].products.push({
            product_id: row.product_id,
            sku: row.sku,
            name: row.name,
            quantity: Number(row.total_quantity ?? 0),
            revenue: Number(row.total_revenue ?? 0)
        });
        groupedData[row.period].totalQuantity += Number(row.total_quantity ?? 0);
        groupedData[row.period].totalRevenue += Number(row.total_revenue ?? 0);
    });

    const result = Object.values(groupedData);
    await cacheService.set(cacheKey, result, 600);

    return result;
};

const getRevenueReport = async ({ period = 'day', startDate = null, endDate = null } = {}) => {
    const cacheKey = `report:revenue:${period}:${startDate || 'all'}:${endDate || 'all'}`;

    const cached = await cacheService.get(cacheKey);
    if (cached) {
        console.log(`[Cache Hit] ${cacheKey}`);
        return cached;
    }

    const data = await reportRepository.getRevenueReport(period, startDate, endDate);

    const result = data.map(row => ({
        period: row.period,
        date: row.date,
        totalOrders: Number(row.total_orders ?? 0),
        totalRevenue: Number(row.total_revenue ?? 0),
        totalItemsSold: Number(row.total_items_sold ?? 0),
        avgOrderValue: Number(row.avg_order_value ?? 0)
    }));

    await cacheService.set(cacheKey, result, 600);

    return result;
};

const getTopSellingProducts = async ({ limit = 10, startDate = null, endDate = null } = {}) => {
    const cacheKey = `report:top:products:${limit}:${startDate || 'all'}:${endDate || 'all'}`;

    const cached = await cacheService.get(cacheKey);
    if (cached) {
        console.log(`[Cache Hit] ${cacheKey}`);
        return cached;
    }

    const data = await reportRepository.getTopSellingProducts(limit, startDate, endDate);

    const result = data.map(row => ({
        product_id: row.product_id,
        sku: row.sku,
        name: row.name,
        price: Number(row.price ?? 0),
        totalQuantity: Number(row.total_quantity ?? 0),
        totalRevenue: Number(row.total_revenue ?? 0),
        timesSold: Number(row.times_sold ?? 0)
    }));

    await cacheService.set(cacheKey, result, 600);

    return result;
};

const getSalesOverview = async ({ startDate = null, endDate = null } = {}) => {
    const cacheKey = `report:overview:${startDate || 'all'}:${endDate || 'all'}`;

    const cached = await cacheService.get(cacheKey);
    if (cached) {
        console.log(`[Cache Hit] ${cacheKey}`);
        return cached;
    }

    const data = await reportRepository.getSalesOverview(startDate, endDate);

    const result = {
        totalOrders: Number(data.total_orders ?? 0),
        totalRevenue: Number(data.total_revenue ?? 0),
        totalItemsSold: Number(data.total_items_sold ?? 0),
        uniqueCustomers: Number(data.unique_customers ?? 0),
        avgOrderValue: Number(data.avg_order_value ?? 0),
        maxOrderValue: Number(data.max_order_value ?? 0),
        minOrderValue: Number(data.min_order_value ?? 0)
    };

    await cacheService.set(cacheKey, result, 600);

    return result;
};

module.exports = {
    getProductSalesReport,
    getRevenueReport,
    getTopSellingProducts,
    getSalesOverview
};
