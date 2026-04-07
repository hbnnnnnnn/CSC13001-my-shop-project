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
    /// Fetches a paginated list of orders and resolves customer names.
    /// </summary>
    public async Task<(List<OrderItem> Orders, int Total, int TotalPages)> GetOrdersAsync(
        int page = 1,
        int limit = 100
    )
    {
        // 1. Fetch orders
        var data = await _graphql.QueryAsync(
            @"query Orders($page: Int, $limit: Int) {
                orders(page: $page, limit: $limit) {
                    data {
                        order_id
                        created_time
                        updated_time
                        final_price
                        status
                        customer_id
                        account_id
                        shipping_address
                        items {
                            order_item_id
                            product_id
                            quantity
                            unit_sale_price
                            total_price
                        }
                    }
                    total
                    page
                    limit
                    totalPages
                }
            }",
            new { page, limit }
        );

        var ordersData = data.GetProperty("orders");
        var total = ordersData.GetProperty("total").GetInt32();
        var totalPages = ordersData.GetProperty("totalPages").GetInt32();

        // 2. Parse orders
        var orderElements = ordersData.GetProperty("data").EnumerateArray().ToList();

        // 3. Collect unique customer IDs and resolve names
        var customerIds = orderElements
            .Where(o => o.TryGetProperty("customer_id", out var c) && c.ValueKind != JsonValueKind.Null)
            .Select(o => o.GetProperty("customer_id").GetString()!)
            .Distinct()
            .ToList();

        var customerMap = new Dictionary<string, string>();
        if (customerIds.Count > 0)
        {
            try
            {
                var custData = await _graphql.QueryAsync(
                    @"query Customers($limit: Int) {
                        customers(limit: $limit) {
                            data { customer_id name }
                        }
                    }",
                    new { limit = 500 }
                );
                foreach (var c in custData.GetProperty("customers").GetProperty("data").EnumerateArray())
                {
                    var id = c.GetProperty("customer_id").GetString()!;
                    var name = c.GetProperty("name").GetString()!;
                    customerMap[id] = name;
                }
            }
            catch
            {
                /* best-effort — show IDs if customers query fails */
            }
        }

        // 4. Map to OrderItem
        var orders = new List<OrderItem>();
        foreach (var o in orderElements)
        {
            orders.Add(MapOrderItem(o, customerMap));
        }

        return (orders, total, totalPages);
    }

    /// <summary>
    /// Fetches a single order by ID with full item details (product names resolved).
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
                    account_id
                    shipping_address
                    items {
                        order_item_id
                        product_id
                        quantity
                        unit_sale_price
                        total_price
                    }
                }
            }",
            new { id }
        );

        var orderEl = data.GetProperty("order");

        // Resolve customer name + phone
        var customerMap = new Dictionary<string, string>();
        string? customerPhone = null;
        if (orderEl.TryGetProperty("customer_id", out var custId) && custId.ValueKind != JsonValueKind.Null)
        {
            var cid = custId.GetString()!;
            try
            {
                var custData = await _graphql.QueryAsync(
                    "query Customer($id: ID!) { customer(id: $id) { customer_id name phone } }",
                    new { id = cid }
                );
                var custNode = custData.GetProperty("customer");
                customerMap[cid] = custNode.GetProperty("name").GetString()!;
                if (custNode.TryGetProperty("phone", out var ph) && ph.ValueKind != JsonValueKind.Null)
                    customerPhone = ph.GetString();
            }
            catch { /* best-effort */ }
        }

        var order = MapOrderItem(orderEl, customerMap);
        if (!string.IsNullOrEmpty(customerPhone))
            order.Phone = customerPhone;

        // Resolve product names for each item
        if (order.Products.Count > 0)
        {
            var productIds = order.Products
                .Select(p => p.ProductId)
                .Where(pid => !string.IsNullOrEmpty(pid))
                .Distinct()
                .ToList();

            var productMap = new Dictionary<string, string>();
            foreach (var pid in productIds)
            {
                try
                {
                    var pData = await _graphql.QueryAsync(
                        "query Product($id: ID!) { product(id: $id) { product_id name } }",
                        new { id = pid }
                    );
                    productMap[pid] = pData.GetProperty("product").GetProperty("name").GetString()!;
                }
                catch { /* best-effort */ }
            }

            foreach (var item in order.Products)
            {
                if (!string.IsNullOrEmpty(item.ProductId) && productMap.TryGetValue(item.ProductId, out var name))
                {
                    item.ProductName = name;
                }
            }
        }

        return order;
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
                    items {
                        product_id
                        quantity
                        unit_sale_price
                        total_price
                    }
                }
            }",
            new { customerId, shippingAddress, items = itemsInput }
        );

        var orderEl = data.GetProperty("createOrder");
        return MapOrderItem(orderEl, new Dictionary<string, string>());
    }

    /// <summary>
    /// Updates the status of an existing order.
    /// </summary>
    public async Task UpdateOrderStatusAsync(string orderId, string status)
    {
        await _graphql.QueryAsync(
            @"mutation UpdateStatus($id: ID!, $status: String!) {
                updateOrderStatus(id: $id, status: $status) {
                    order_id
                    status
                }
            }",
            new { id = orderId, status }
        );
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

    private static OrderItem MapOrderItem(JsonElement o, Dictionary<string, string> customerMap)
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

        // Resolve customer name
        var customerName = customerId != null && customerMap.TryGetValue(customerId, out var name)
            ? name
            : (customerId ?? "—");

        // Parse date
        var dateStr = "";
        if (!string.IsNullOrEmpty(createdTime))
        {
            if (DateTimeOffset.TryParse(createdTime, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dto))
                dateStr = dto.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
            else
                dateStr = createdTime;
        }

        // Parse items
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

                products.Add(new OrderProductItem
                {
                    ProductId = productId,
                    ProductName = $"Product #{productId}",  // placeholder, resolved later for detail view
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
            Phone = "",
            Email = "",
            Address = shippingAddress,
            ShippingFee = 0m,
            Products = products,
        };
    }
}
