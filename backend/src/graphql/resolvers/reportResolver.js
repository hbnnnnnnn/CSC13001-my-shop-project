const reportService = require('../../services/report.service.js');
const { requireRole } = require('../../middlewares/auth.middleware.js');

const reportResolver = {
    Query: {
        categorySalesReport: requireRole(['Admin', 'Sale'], async (_, { period = 'day', startDate = null, endDate = null }) => {
            return await reportService.getCategorySalesReport({ period, startDate, endDate });
        }),

        productSalesReport: requireRole(['Admin', 'Sale'], async (_, { period = 'day', startDate = null, endDate = null, categoryId = null }) => {
            return await reportService.getProductSalesReport({ period, startDate, endDate, categoryId });
        }),

        revenueReport: requireRole(['Admin', 'Sale'], async (_, { period = 'day', startDate = null, endDate = null }) => {
            return await reportService.getRevenueReport({ period, startDate, endDate });
        }),

        topSellingProductsReport: requireRole(['Admin', 'Sale'], async (_, { limit = 10, startDate = null, endDate = null, categoryId = null }) => {
            return await reportService.getTopSellingProducts({ limit, startDate, endDate, categoryId });
        }),

        salesOverview: requireRole(['Admin', 'Sale'], async (_, { startDate = null, endDate = null }) => {
            return await reportService.getSalesOverview({ startDate, endDate });
        })
    }
};

module.exports = reportResolver;
