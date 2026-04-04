const db = require("../config/db.js");

class ReportRepository {
    constructor() {
        this.db = db;
    }

    async getProductSalesReport(period = 'day', startDate = null, endDate = null) {
        let dateFormat;
        let dateExpr;
        let groupByClause;

        switch (period) {
            case 'week':
                dateFormat = 'YYYY-IW';
                dateExpr = `DATE_TRUNC('week', o.created_time)::DATE`;
                groupByClause = `TO_CHAR(o.created_time, 'YYYY-IW'), DATE_TRUNC('week', o.created_time)::DATE`;
                break;
            case 'month':
                dateFormat = 'YYYY-MM';
                dateExpr = `DATE_TRUNC('month', o.created_time)::DATE`;
                groupByClause = `TO_CHAR(o.created_time, 'YYYY-MM'), DATE_TRUNC('month', o.created_time)::DATE`;
                break;
            case 'year':
                dateFormat = 'YYYY';
                dateExpr = `DATE_TRUNC('year', o.created_time)::DATE`;
                groupByClause = `TO_CHAR(o.created_time, 'YYYY'), DATE_TRUNC('year', o.created_time)::DATE`;
                break;
            case 'day':
            default:
                dateFormat = 'YYYY-MM-DD';
                dateExpr = `o.created_time::DATE`;
                groupByClause = `TO_CHAR(o.created_time, 'YYYY-MM-DD'), o.created_time::DATE`;
                break;
        }

        let whereClause = `WHERE o.status = 'Paid'`;
        const params = [];

        if (startDate) {
            whereClause += ` AND o.created_time::DATE >= $${params.length + 1}`;
            params.push(startDate);
        }

        if (endDate) {
            whereClause += ` AND o.created_time::DATE <= $${params.length + 1}`;
            params.push(endDate);
        }

        const query = `
            SELECT 
                TO_CHAR(o.created_time, '${dateFormat}') AS period,
                ${dateExpr} AS date,
                p.product_id,
                p.sku,
                p.name,
                SUM(oi.quantity) AS total_quantity,
                SUM(oi.total_price) AS total_revenue
            FROM orders o
            JOIN order_item oi ON o.order_id = oi.order_id
            JOIN product p ON oi.product_id = p.product_id
            ${whereClause}
            GROUP BY ${groupByClause}, p.product_id, p.sku, p.name
            ORDER BY ${dateExpr} DESC, p.product_id
        `;

        const result = await this.db.query(query, params);
        return result.rows;
    }

    async getRevenueReport(period = 'day', startDate = null, endDate = null) {
        let dateFormat;
        let dateExpr;
        let groupByClause;

        switch (period) {
            case 'week':
                dateFormat = 'YYYY-IW';
                dateExpr = `DATE_TRUNC('week', o.created_time)::DATE`;
                groupByClause = `TO_CHAR(o.created_time, 'YYYY-IW'), DATE_TRUNC('week', o.created_time)::DATE`;
                break;
            case 'month':
                dateFormat = 'YYYY-MM';
                dateExpr = `DATE_TRUNC('month', o.created_time)::DATE`;
                groupByClause = `TO_CHAR(o.created_time, 'YYYY-MM'), DATE_TRUNC('month', o.created_time)::DATE`;
                break;
            case 'year':
                dateFormat = 'YYYY';
                dateExpr = `DATE_TRUNC('year', o.created_time)::DATE`;
                groupByClause = `TO_CHAR(o.created_time, 'YYYY'), DATE_TRUNC('year', o.created_time)::DATE`;
                break;
            case 'day':
            default:
                dateFormat = 'YYYY-MM-DD';
                dateExpr = `o.created_time::DATE`;
                groupByClause = `TO_CHAR(o.created_time, 'YYYY-MM-DD'), o.created_time::DATE`;
                break;
        }

        let whereClause = `WHERE o.status = 'Paid'`;
        const params = [];

        if (startDate) {
            whereClause += ` AND o.created_time::DATE >= $${params.length + 1}`;
            params.push(startDate);
        }

        if (endDate) {
            whereClause += ` AND o.created_time::DATE <= $${params.length + 1}`;
            params.push(endDate);
        }

        const query = `
            SELECT 
                TO_CHAR(o.created_time, '${dateFormat}') AS period,
                ${dateExpr} AS date,
                COUNT(DISTINCT o.order_id) AS total_orders,
                SUM(o.final_price) AS total_revenue,
                SUM(oi.quantity) AS total_items_sold,
                ROUND(AVG(o.final_price)::numeric, 2) AS avg_order_value
            FROM orders o
            LEFT JOIN order_item oi ON o.order_id = oi.order_id
            ${whereClause}
            GROUP BY ${groupByClause}
            ORDER BY ${dateExpr} DESC
        `;

        const result = await this.db.query(query, params);
        return result.rows;
    }

    async getTopSellingProducts(limit = 10, startDate = null, endDate = null) {
        let whereClause = `WHERE o.status = 'Paid'`;
        const params = [limit];

        if (startDate) {
            whereClause += ` AND o.created_time::DATE >= $${params.length + 1}`;
            params.push(startDate);
        }

        if (endDate) {
            whereClause += ` AND o.created_time::DATE <= $${params.length + 1}`;
            params.push(endDate);
        }

        const query = `
            SELECT 
                p.product_id,
                p.sku,
                p.name,
                p.price,
                SUM(oi.quantity) AS total_quantity,
                SUM(oi.total_price) AS total_revenue,
                COUNT(DISTINCT o.order_id) AS times_sold
            FROM product p
            JOIN order_item oi ON p.product_id = oi.product_id
            JOIN orders o ON oi.order_id = o.order_id
            ${whereClause}
            GROUP BY p.product_id, p.sku, p.name, p.price
            ORDER BY total_quantity DESC
            LIMIT $1
        `;

        const result = await this.db.query(query, params);
        return result.rows;
    }

    async getSalesOverview(startDate = null, endDate = null) {
        let whereClause = `WHERE o.status = 'Paid'`;
        const params = [];

        if (startDate) {
            whereClause += ` AND o.created_time::DATE >= $${params.length + 1}`;
            params.push(startDate);
        }

        if (endDate) {
            whereClause += ` AND o.created_time::DATE <= $${params.length + 1}`;
            params.push(endDate);
        }

        const query = `
            SELECT 
                COUNT(DISTINCT o.order_id) AS total_orders,
                SUM(o.final_price) AS total_revenue,
                SUM(oi.quantity) AS total_items_sold,
                COUNT(DISTINCT o.customer_id) AS unique_customers,
                ROUND(AVG(o.final_price)::numeric, 2) AS avg_order_value,
                MAX(o.final_price) AS max_order_value,
                MIN(o.final_price) AS min_order_value
            FROM orders o
            LEFT JOIN order_item oi ON o.order_id = oi.order_id
            ${whereClause}
        `;

        const result = await this.db.query(query, params);
        return result.rows[0];
    }
}

module.exports = new ReportRepository();
