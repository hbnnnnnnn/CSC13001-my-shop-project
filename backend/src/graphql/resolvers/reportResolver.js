const reportService = require('../../services/report.service.js');
const { requireRole } = require('../../middlewares/auth.middleware.js');

const reportResolver = {
    Query: {
        productSalesReport: requireRole(['Admin', 'Sale'], async (_, { period = 'day', startDate = null, endDate = null }) => {
            return await reportService.getProductSalesReport({ period, startDate, endDate });
        }),

        revenueReport: requireRole(['Admin', 'Sale'], async (_, { period = 'day', startDate = null, endDate = null }) => {
            return await reportService.getRevenueReport({ period, startDate, endDate });
        }),

        topSellingProducts: requireRole(['Admin', 'Sale'], async (_, { limit = 10, startDate = null, endDate = null }) => {
            return await reportService.getTopSellingProducts({ limit, startDate, endDate });
        }),

        salesOverview: requireRole(['Admin', 'Sale'], async (_, { startDate = null, endDate = null }) => {
            return await reportService.getSalesOverview({ startDate, endDate });
        })
    }
};

module.exports = reportResolver;
