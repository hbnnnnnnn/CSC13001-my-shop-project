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

        let whereClause = `WHERE o.status = 'Delivered' AND o.is_deleted = false`;
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

        let whereClause = `WHERE o.status = 'Delivered' AND o.is_deleted = false`;
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
            WITH order_metrics AS (
                SELECT
                    TO_CHAR(o.created_time, '${dateFormat}') AS period,
                    ${dateExpr} AS date,
                    o.order_id,
                    o.final_price
                FROM orders o
                ${whereClause}
            ),
            item_metrics AS (
                SELECT
                    TO_CHAR(o.created_time, '${dateFormat}') AS period,
                    ${dateExpr} AS date,
                    COALESCE(SUM(oi.quantity), 0) AS total_items_sold
                FROM orders o
                LEFT JOIN order_item oi ON o.order_id = oi.order_id
                ${whereClause}
                GROUP BY ${groupByClause}
            )
            SELECT
                om.period,
                om.date,
                COUNT(om.order_id) AS total_orders,
                COALESCE(SUM(om.final_price), 0) AS total_revenue,
                COALESCE(im.total_items_sold, 0) AS total_items_sold,
                ROUND(AVG(om.final_price)::numeric, 2) AS avg_order_value
            FROM order_metrics om
            LEFT JOIN item_metrics im ON im.period = om.period AND im.date = om.date
            GROUP BY om.period, om.date, im.total_items_sold
            ORDER BY om.date DESC
        `;

        const result = await this.db.query(query, params);
        return result.rows;
    }

    async getTopSellingProducts(limit = 10, startDate = null, endDate = null) {
        let whereClause = `WHERE o.status = 'Delivered' AND o.is_deleted = false`;
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
        let whereClause = `WHERE o.status = 'Delivered' AND o.is_deleted = false`;
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
                orders_agg.total_orders,
                orders_agg.total_revenue,
                items_agg.total_items_sold,
                orders_agg.unique_customers,
                orders_agg.avg_order_value,
                orders_agg.max_order_value,
                orders_agg.min_order_value
            FROM (
                SELECT
                    COUNT(*) AS total_orders,
                    COALESCE(SUM(o.final_price), 0) AS total_revenue,
                    COUNT(DISTINCT o.customer_id) AS unique_customers,
                    ROUND(AVG(o.final_price)::numeric, 2) AS avg_order_value,
                    COALESCE(MAX(o.final_price), 0) AS max_order_value,
                    COALESCE(MIN(o.final_price), 0) AS min_order_value
                FROM orders o
                ${whereClause}
            ) AS orders_agg
            CROSS JOIN (
                SELECT
                    COALESCE(SUM(oi.quantity), 0) AS total_items_sold
                FROM orders o
                LEFT JOIN order_item oi ON o.order_id = oi.order_id
                ${whereClause}
            ) AS items_agg
        `;

        const result = await this.db.query(query, params);
        return result.rows[0];
    }
}

module.exports = new ReportRepository();
