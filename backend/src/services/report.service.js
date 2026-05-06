const reportRepository = require('../repositories/report.repository.js');
const cacheService = require('../utils/cache.util.js');

const REPORT_CACHE_TTL_SECONDS = 300;

const normalizeReportDate = (value) => {
    if (value == null) return null;

    if (value instanceof Date) {
        return value.toISOString();
    }

    if (typeof value === 'number') {
        return new Date(value).toISOString();
    }

    if (typeof value === 'string') {
        if (/^\d+$/.test(value)) {
            const asNumber = Number(value);
            if (!Number.isNaN(asNumber)) {
                return new Date(asNumber).toISOString();
            }
        }
        return value;
    }

    return String(value);
};

const getCategorySalesReport = async ({ period = 'day', startDate = null, endDate = null } = {}) => {
    const cacheKey = `report:categories:${period}:${startDate || 'all'}:${endDate || 'all'}`;

    const cached = await cacheService.get(cacheKey);
    if (cached) {
        console.log(`[Cache Hit] ${cacheKey}`);
        return cached;
    }

    const data = await reportRepository.getCategorySalesReport(period, startDate, endDate);
    const groupedData = {};

    data.forEach(row => {
        if (!groupedData[row.period]) {
            groupedData[row.period] = {
                period: row.period,
                date: normalizeReportDate(row.date),
                categories: [],
                totalQuantity: 0,
                totalRevenue: 0,
                totalCost: 0
            };
        }

        const catRevenue = Number(row.total_revenue ?? 0);
        const catCost = Number(row.total_cost ?? 0);

        groupedData[row.period].categories.push({
            category_id: row.category_id,
            category_name: row.category_name,
            quantity: Number(row.total_quantity ?? 0),
            revenue: catRevenue,
            totalCost: catCost,
            totalProfit: catRevenue - catCost
        });

        groupedData[row.period].totalQuantity += Number(row.total_quantity ?? 0);
        groupedData[row.period].totalRevenue += catRevenue;
        groupedData[row.period].totalCost = (groupedData[row.period].totalCost ?? 0) + catCost;
    });

    const result = Object.values(groupedData).map(period => ({
        ...period,
        totalProfit: period.totalRevenue - period.totalCost
    }));
    await cacheService.set(cacheKey, result, REPORT_CACHE_TTL_SECONDS);

    return result;
};

const getProductSalesReport = async ({ period = 'day', startDate = null, endDate = null, categoryId = null } = {}) => {
    const cacheKey = `report:products:${period}:${startDate || 'all'}:${endDate || 'all'}:${categoryId || 'all'}`;

    const cached = await cacheService.get(cacheKey);
    if (cached) {
        console.log(`[Cache Hit] ${cacheKey}`);
        return cached;
    }

    const data = await reportRepository.getProductSalesReport(period, startDate, endDate, categoryId);
    const groupedData = {};
    data.forEach(row => {
        if (!groupedData[row.period]) {
            groupedData[row.period] = {
                period: row.period,
                date: normalizeReportDate(row.date),
                products: [],
                totalQuantity: 0,
                totalRevenue: 0,
                totalCost: 0
            };
        }
        const prodRevenue = Number(row.total_revenue ?? 0);
        const prodCost = Number(row.total_cost ?? 0);
        groupedData[row.period].products.push({
            product_id: row.product_id,
            sku: row.sku,
            name: row.name,
            quantity: Number(row.total_quantity ?? 0),
            revenue: prodRevenue,
            totalCost: prodCost,
            totalProfit: prodRevenue - prodCost
        });
        groupedData[row.period].totalQuantity += Number(row.total_quantity ?? 0);
        groupedData[row.period].totalRevenue += prodRevenue;
        groupedData[row.period].totalCost += prodCost;
    });

    const result = Object.values(groupedData).map(period => ({
        ...period,
        totalProfit: period.totalRevenue - period.totalCost
    }));
    await cacheService.set(cacheKey, result, REPORT_CACHE_TTL_SECONDS);

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
        date: normalizeReportDate(row.date),
        totalOrders: Number(row.total_orders ?? 0),
        totalRevenue: Number(row.total_revenue ?? 0),
        totalCost: Number(row.total_cost ?? 0),
        totalProfit: Number(row.total_profit ?? 0),
        totalItemsSold: Number(row.total_items_sold ?? 0),
        avgOrderValue: Number(row.avg_order_value ?? 0)
    }));

    await cacheService.set(cacheKey, result, REPORT_CACHE_TTL_SECONDS);

    return result;
};

const getTopSellingProducts = async ({ limit = 10, startDate = null, endDate = null, categoryId = null } = {}) => {
    const cacheKey = `report:top:products:${limit}:${startDate || 'all'}:${endDate || 'all'}:${categoryId || 'all'}`;

    const cached = await cacheService.get(cacheKey);
    if (cached) {
        console.log(`[Cache Hit] ${cacheKey}`);
        return cached;
    }

    const data = await reportRepository.getTopSellingProducts(limit, startDate, endDate, categoryId);

    const result = data.map(row => {
        const totalRevenue = Number(row.total_revenue ?? 0);
        const totalCost = Number(row.total_cost ?? 0);
        return {
            product_id: row.product_id,
            sku: row.sku,
            name: row.name,
            price: Number(row.price ?? 0),
            costPrice: Number(row.cost_price ?? 0),
            totalQuantity: Number(row.total_quantity ?? 0),
            totalRevenue,
            totalCost,
            totalProfit: totalRevenue - totalCost,
            timesSold: Number(row.times_sold ?? 0)
        };
    });

    await cacheService.set(cacheKey, result, REPORT_CACHE_TTL_SECONDS);

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

    await cacheService.set(cacheKey, result, REPORT_CACHE_TTL_SECONDS);

    return result;
};

module.exports = {
    getCategorySalesReport,
    getProductSalesReport,
    getRevenueReport,
    getTopSellingProducts,
    getSalesOverview
};
