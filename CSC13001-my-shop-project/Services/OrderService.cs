using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.Json;
using CSC13001_my_shop_project.Presentation.OrderList;

namespace CSC13001_my_shop_project.Services;

/// <summary>
/// Picker-friendly models for ComboBox/AutoSuggest display.
/// </summary>
public record ProductPickerItem(string ProductId, string Name, int Price, int Stock)
{
    public string DisplayText => $"{Name}  ({Price:N0} ₫, tồn: {Stock})";
    public override string ToString() => DisplayText;
}

public record CustomerPickerItem(string CustomerId, string Name, string? Phone, string? Address)
{
    public string DisplayText => string.IsNullOrEmpty(Phone) ? Name : $"{Name} — {Phone}";
    public override string ToString() => DisplayText;
}

/// <summary>
/// Service layer for Order-related GraphQL operations.
/// Follows the same pattern as <see cref="AuthService"/>.
/// </summary>
public class OrderService
{
    private readonly GraphqlService _graphql;

    public OrderService(GraphqlService graphql)
    {
        _graphql = graphql;
    }

    // ────────────────────────────────────────────────────
    // ORDER QUERIES
    // ────────────────────────────────────────────────────

    /// <summary>
    /// Fetches a paginated list of orders with nested customer data.
    /// Supports server-side filter (status, date range) and sort.
    /// </summary>
    public async Task<(List<OrderItem> Orders, int Total, int TotalPages)> GetOrdersAsync(
        int page = 1,
        int limit = 100,
        string? statusFilter = null,
        string? startDate = null,
        string? endDate = null,
        string? sortField = null,
        string? sortOrder = null
    )
    {
        // Build filter input — always send object (backend crashes on null)
        var filterDict = new Dictionary<string, object>();
        if (statusFilter != null) filterDict["status"] = statusFilter;
        if (startDate != null) filterDict["startDate"] = startDate;
        if (endDate != null) filterDict["endDate"] = endDate;
        object filter = filterDict;

        // Build sort input
        object? sort = null;
        if (sortField != null && sortOrder != null)
        {
            sort = new { field = sortField, order = sortOrder };
        }

        var data = await _graphql.QueryAsync(
            @"query Orders($page: Int, $limit: Int, $filter: OrderFilterInput, $sort: OrderSortInput) {
                orders(page: $page, limit: $limit, filter: $filter, sort: $sort) {
                    data {
                        order_id
                        created_time
                        updated_time
                        final_price
                        status
                        customer_id
                        shipping_address
                        customer { name phone }
                        items {
                            order_item_id
                            product_id
                            quantity
                            unit_sale_price
                            total_price
                            product { name }
                        }
                    }
                    total
                    page
                    limit
                    totalPages
                }
            }",
            new { page, limit, filter, sort }
        );

        var ordersData = data.GetProperty("orders");
        var total = ordersData.GetProperty("total").GetInt32();
        var totalPages = ordersData.GetProperty("totalPages").GetInt32();

        var orders = ordersData.GetProperty("data").EnumerateArray()
            .Select(MapOrderItem)
            .ToList();

        return (orders, total, totalPages);
    }

    /// <summary>
    /// Fetches a single order by ID with full details.
    /// Uses backend nested resolvers — customer + product names resolved in one query.
    /// </summary>
    public async Task<OrderItem> GetOrderByIdAsync(string id)
    {
        var data = await _graphql.QueryAsync(
            @"query Order($id: ID!) {
                order(id: $id) {
                    order_id
                    created_time
                    updated_time
                    final_price
                    status
                    customer_id
                    shipping_address
                    customer { name phone address }
                    items {
                        order_item_id
                        product_id
                        quantity
                        unit_sale_price
                        total_price
                        product { name }
                    }
                }
            }",
            new { id }
        );

        return MapOrderItem(data.GetProperty("order"));
    }

    // ────────────────────────────────────────────────────
    // ORDER MUTATIONS
    // ────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new order via the backend.
    /// </summary>
    public async Task<OrderItem> CreateOrderAsync(
        string? customerId,
        string? shippingAddress,
        List<(string ProductId, int Quantity)> items
    )
    {
        var itemsInput = items.Select(i => new { product_id = i.ProductId, quantity = i.Quantity }).ToArray();

        var data = await _graphql.QueryAsync(
            @"mutation CreateOrder($customerId: ID, $shippingAddress: String, $items: [OrderItemInput!]!) {
                createOrder(customer_id: $customerId, shipping_address: $shippingAddress, items: $items) {
                    order_id
                    created_time
                    final_price
                    status
                    customer_id
                    shipping_address
                    customer { name phone }
                    items {
                        product_id
                        quantity
                        unit_sale_price
                        total_price
                        product { name }
                    }
                }
            }",
            new { customerId, shippingAddress, items = itemsInput }
        );

        return MapOrderItem(data.GetProperty("createOrder"));
    }

    /// Full update of an order: status, info, and/or items via <c>updateOrderFull</c>.
    /// This is the recommended mutation (replaces deprecated updateOrderStatus/updateOrder).
    /// </summary>
    public async Task<OrderItem> UpdateOrderFullAsync(
        string orderId,
        string? status = null,
        string? shippingAddress = null,
        string? recipientName = null,
        string? recipientPhone = null,
        string? recipientEmail = null,
        List<(string ProductId, int Quantity)>? items = null
    )
    {
        // Build input object – only include non-null fields
        var input = new Dictionary<string, object?>();
        if (status is not null) input["status"] = status;
        if (shippingAddress is not null) input["shipping_address"] = shippingAddress;
        if (recipientName is not null) input["recipient_name"] = recipientName;
        if (recipientPhone is not null) input["recipient_phone"] = recipientPhone;
        if (recipientEmail is not null) input["recipient_email"] = recipientEmail;
        if (items is not null)
        {
            input["items"] = items.Select(i => new { product_id = i.ProductId, quantity = i.Quantity }).ToArray();
        }

        var data = await _graphql.QueryAsync(
            @"mutation UpdateOrderFull($id: ID!, $input: UpdateOrderFullInput!) {
                updateOrderFull(id: $id, input: $input) {
                    order_id
                    created_time
                    updated_time
                    final_price
                    status
                    customer_id
                    shipping_address
                    recipient_name
                    recipient_phone
                    recipient_email
                    customer { name phone address }
                    items {
                        order_item_id
                        product_id
                        quantity
                        unit_sale_price
                        total_price
                        product { name }
                    }
                }
            }",
            new { id = orderId, input }
        );

        return MapOrderItem(data.GetProperty("updateOrderFull"));
    }

    /// <summary>
    /// Soft-deletes an order via <c>deleteOrder</c>.
    /// Only works for orders with status Created or Processing.
    /// </summary>
    public async Task<bool> DeleteOrderAsync(string orderId)
    {
        var data = await _graphql.QueryAsync(
            @"mutation DeleteOrder($id: ID!) {
                deleteOrder(id: $id)
            }",
            new { id = orderId }
        );

        return data.GetProperty("deleteOrder").GetBoolean();
    }

    /// <summary>
    /// Updates the status of an existing order.
    /// </summary>
    [Obsolete("Use UpdateOrderFullAsync instead")]
    public async Task UpdateOrderStatusAsync(string orderId, string status)
    {
        await UpdateOrderFullAsync(orderId, status: status);
    }

    // ────────────────────────────────────────────────────
    // PICKER DATA
    // ────────────────────────────────────────────────────

    /// <summary>
    /// Fetches all products for the product picker ComboBox.
    /// </summary>
    public async Task<List<ProductPickerItem>> GetProductsForPickerAsync()
    {
        var data = await _graphql.QueryAsync(
            @"query Products {
                products(page: 1, limit: 200) {
                    data { product_id name price stock }
                }
            }"
        );

        var items = new List<ProductPickerItem>();
        foreach (var p in data.GetProperty("products").GetProperty("data").EnumerateArray())
        {
            items.Add(new ProductPickerItem(
                ProductId: p.GetProperty("product_id").GetString()!,
                Name: p.GetProperty("name").GetString()!,
                Price: p.GetProperty("price").GetInt32(),
                Stock: p.GetProperty("stock").GetInt32()
            ));
        }

        return items;
    }

    /// <summary>
    /// Fetches all customers for the customer picker ComboBox.
    /// </summary>
    public async Task<List<CustomerPickerItem>> GetCustomersForPickerAsync()
    {
        var data = await _graphql.QueryAsync(
            @"query Customers {
                customers(page: 1, limit: 500) {
                    data { customer_id name phone address }
                }
            }"
        );

        var items = new List<CustomerPickerItem>();
        foreach (var c in data.GetProperty("customers").GetProperty("data").EnumerateArray())
        {
            items.Add(new CustomerPickerItem(
                CustomerId: c.GetProperty("customer_id").GetString()!,
                Name: c.GetProperty("name").GetString()!,
                Phone: c.TryGetProperty("phone", out var ph) && ph.ValueKind != JsonValueKind.Null
                    ? ph.GetString()
                    : null,
                Address: c.TryGetProperty("address", out var addr) && addr.ValueKind != JsonValueKind.Null
                    ? addr.GetString()
                    : null
            ));
        }

        return items;
    }

    // ────────────────────────────────────────────────────
    // MAPPING HELPERS
    // ────────────────────────────────────────────────────

    /// <summary>
    /// Maps a GraphQL Order JSON element to an OrderItem model.
    /// Reads nested customer/product data directly — no separate resolution needed.
    /// </summary>
    private static OrderItem MapOrderItem(JsonElement o)
    {
        var orderId = o.GetProperty("order_id").GetString()!;
        var status = o.GetProperty("status").GetString() ?? "Unknown";
        var finalPrice = o.GetProperty("final_price").GetInt32();
        var createdTime = o.TryGetProperty("created_time", out var ct) && ct.ValueKind != JsonValueKind.Null
            ? ct.GetString()
            : null;
        var customerId = o.TryGetProperty("customer_id", out var cid) && cid.ValueKind != JsonValueKind.Null
            ? cid.GetString()
            : null;
        var shippingAddress = o.TryGetProperty("shipping_address", out var sa) && sa.ValueKind != JsonValueKind.Null
            ? sa.GetString() ?? ""
            : "";

        // Read nested customer data
        var customerName = "—";
        var customerPhone = "";
        var customerAddress = "";
        if (o.TryGetProperty("customer", out var custEl) && custEl.ValueKind == JsonValueKind.Object)
        {
            customerName = custEl.TryGetProperty("name", out var cn) && cn.ValueKind != JsonValueKind.Null
                ? cn.GetString() ?? "—"
                : "—";
            customerPhone = custEl.TryGetProperty("phone", out var cp) && cp.ValueKind != JsonValueKind.Null
                ? cp.GetString() ?? ""
                : "";
            // Use customer address as fallback if shipping_address is empty
            if (string.IsNullOrEmpty(shippingAddress))
            {
                customerAddress = custEl.TryGetProperty("address", out var ca) && ca.ValueKind != JsonValueKind.Null
                    ? ca.GetString() ?? ""
                    : "";
            }
        }

        // Parse date — handle multiple formats from backend
        var dateStr = "";
        if (!string.IsNullOrEmpty(createdTime))
        {
            // Try 1: Standard ISO / human-readable formats
            if (DateTimeOffset.TryParse(createdTime, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dto))
            {
                dateStr = dto.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
            }
            // Try 2: Unix timestamp in milliseconds (e.g. "1714444800000")
            else if (long.TryParse(createdTime, out var unixMs) && unixMs > 1_000_000_000_000)
            {
                var dt = DateTimeOffset.FromUnixTimeMilliseconds(unixMs);
                dateStr = dt.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
            }
            // Try 3: Unix timestamp in seconds (e.g. "1714444800")
            else if (long.TryParse(createdTime, out var unixSec) && unixSec > 1_000_000_000)
            {
                var dt = DateTimeOffset.FromUnixTimeSeconds(unixSec);
                dateStr = dt.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
            }
            // Fallback: try DateTime.TryParse with relaxed settings (handles "Wed Apr 30 2026 ..." etc.)
            else if (DateTime.TryParse(createdTime, CultureInfo.InvariantCulture,
                         DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeUniversal, out var dtFallback))
            {
                dateStr = dtFallback.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
            }
            else
            {
                // Last resort: show something meaningful
                dateStr = createdTime.Length > 10 ? createdTime[..10] : createdTime;
            }
        }

        // Parse items with nested product names
        var products = new ObservableCollection<OrderProductItem>();
        if (o.TryGetProperty("items", out var itemsEl) && itemsEl.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in itemsEl.EnumerateArray())
            {
                var productId = item.TryGetProperty("product_id", out var pid) && pid.ValueKind != JsonValueKind.Null
                    ? pid.GetString() ?? ""
                    : "";
                var quantity = item.GetProperty("quantity").GetInt32();
                var unitPrice = item.GetProperty("unit_sale_price").GetInt32();

                // Read nested product name directly
                var productName = $"Product #{productId}";
                if (item.TryGetProperty("product", out var prodEl) && prodEl.ValueKind == JsonValueKind.Object)
                {
                    productName = prodEl.TryGetProperty("name", out var pn) && pn.ValueKind != JsonValueKind.Null
                        ? pn.GetString() ?? productName
                        : productName;
                }

                products.Add(new OrderProductItem
                {
                    ProductId = productId,
                    ProductName = productName,
                    Quantity = quantity,
                    UnitPrice = unitPrice,
                });
            }
        }

        return new OrderItem
        {
            Id = $"#{orderId}",
            CustomerId = customerId ?? "",
            CustomerName = customerName,
            Date = dateStr,
            Status = status,
            Amount = $"{finalPrice:N0} ₫",
            Phone = customerPhone,
            Email = "",
            Address = string.IsNullOrEmpty(shippingAddress) ? customerAddress : shippingAddress,
            ShippingFee = 0m,
            Products = products,
        };
    }
}
