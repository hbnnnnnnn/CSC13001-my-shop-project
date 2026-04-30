# Apollo Demo Queries & Mutations (copy de test)

Ngay tao: 2026-04-06

Tai lieu nay tong hop query/mutation dua theo schema + resolver trong du an.
Ban co the copy tung doan vao Apollo Sandbox / Apollo Studio Explorer de test nhanh.

## 1) Cau hinh Header cho endpoint can auth

Sau khi login/register lay token, them header sau trong tab Headers:

```json
{
  "Authorization": "Bearer <YOUR_JWT_TOKEN>"
}
```

Luu y phan quyen theo resolver:
- Admin: duoc tao/sua/xoa category, customer
- Admin hoac Sale: orders + reports
- Me, Logout: can dang nhap

## 2) Account (register, login, me, logout)

### 2.1 Register

```graphql
mutation RegisterAdmin {
  register(
    username: "admin_demo"
    password: "123456"
    full_name: "Admin Demo"
    account_role: "Admin"
  ) {
    token
    account {
      account_id
      username
      full_name
      account_role
    }
  }
}
```

### 2.2 Login

```graphql
mutation Login {
  login(username: "admin_demo", password: "123456") {
    token
    account {
      account_id
      username
      full_name
      account_role
    }
  }
}
```

### 2.3 Me (can header Bearer token)

```graphql
query Me {
  me {
    account_id
    username
    full_name
    account_role
  }
}
```

### 2.4 Logout (can header Bearer token)

```graphql
mutation Logout {
  logout
}
```

## 3) Category

### 3.1 Tao category (Admin)

```graphql
mutation CreateCategory {
  createCategory(name: "Laptop", description: "Nhom laptop") {
    category_id
    name
    description
  }
}
```

### 3.2 Lay danh sach category

```graphql
query Categories {
  categories(page: 1, limit: 10) {
    data {
      category_id
      name
      description
    }
    total
    page
    limit
    totalPages
  }
}
```

### 3.3 Lay category theo id

```graphql
query CategoryById {
  category(id: "1") {
    category_id
    name
    description
  }
}
```

### 3.4 Cap nhat category (Admin)

```graphql
mutation UpdateCategory {
  updateCategory(id: "1", name: "Laptop Gaming", description: "Cap nhat mo ta") {
    category_id
    name
    description
  }
}
```

### 3.5 Xoa category (Admin)

```graphql
mutation DeleteCategory {
  deleteCategory(id: "1")
}
```

## 4) Customer

### 4.1 Tao customer (Admin)

```graphql
mutation CreateCustomer {
  createCustomer(name: "Nguyen Van A", email: "a@gmail.com", phone: "0900000001", address: "HCM") {
    customer_id
    name
    email
    phone
    address
  }
}
```

### 4.2 Danh sach customer

```graphql
query Customers {
  customers(page: 1, limit: 10) {
    data {
      customer_id
      name
      email
      phone
      address
    }
    total
    page
    limit
    totalPages
  }
}
```

### 4.3 Customer theo id

```graphql
query CustomerById {
  customer(id: "1") {
    customer_id
    name
    email
    phone
    address
  }
}
```

### 4.4 Customer theo phone

```graphql
query CustomerByPhone {
  customerByPhone(phone: "0900000001") {
    customer_id
    name
    email
    phone
    address
  }
}
```

### 4.5 Cap nhat customer (Admin)

```graphql
mutation UpdateCustomer {
  updateCustomer(id: "1", name: "Nguyen Van B", email: "b@gmail.com", phone: "0900000002", address: "Ha Noi") {
    customer_id
    name
    email
    phone
    address
  }
}
```

### 4.6 Xoa customer (Admin)

```graphql
mutation DeleteCustomer {
  deleteCustomer(id: "1")
}
```

## 5) Product

### 5.1 Tao product

```graphql
mutation CreateProduct {
  createProduct(
    input: {
      sku: "SKU-DEMO-001"
      name: "Laptop Demo"
      price: 25000000
      stock: 30
      description: "May demo"
      images: ["https://example.com/p1.jpg"]
      supplier: "DemoSupplier"
      category_id: "1"
    }
  ) {
    product_id
    sku
    name
    price
    stock
    description
    images
    supplier
    category {
      category_id
      name
    }
    created_time
    updated_time
  }
}
```

### 5.2 Danh sach product (co filter + sort)

```graphql
query Products {
  products(
    page: 1
    limit: 10
    filter: { category_id: "1", min_price: 1000000, max_price: 50000000 }
    sort: { field: PRICE, order: DESC }
  ) {
    data {
      product_id
      sku
      name
      price
      stock
      supplier
      category {
        category_id
        name
      }
    }
    total
    page
    limit
    totalPages
  }
}
```

### 5.3 Product theo id

```graphql
query ProductById {
  product(id: "1") {
    product_id
    sku
    name
    price
    stock
    description
    images
    supplier
    category {
      category_id
      name
    }
    created_time
    updated_time
  }
}
```

### 5.4 Top low stock products

```graphql
query TopLowStock {
  topLowStockProducts(limit: 5) {
    product_id
    sku
    name
    price
    stock
  }
}
```

### 5.5 Top selling products (phien ban Product)

```graphql
query TopSellingProductsProductModule {
  topSellingProducts(limit: 5) {
    product_id
    sku
    name
    price
    stock
  }
}
```

### 5.6 Tim kiem product

```graphql
query ProductSearch {
  productSearch(
    query: "laptop"
    page: 1
    limit: 10
    filter: { min_price: 5000000, max_price: 50000000 }
    sort: { field: NAME, order: ASC }
  ) {
    data {
      product_id
      sku
      name
      price
      stock
      supplier
    }
    total
    page
    limit
    totalPages
  }
}
```

### 5.7 Cap nhat product

```graphql
mutation UpdateProduct {
  updateProduct(
    id: "1"
    input: {
      name: "Laptop Demo V2"
      price: 26000000
      stock: 25
      description: "Cap nhat demo"
      supplier: "DemoSupplier2"
      category_id: "1"
    }
  ) {
    product_id
    sku
    name
    price
    stock
    description
    supplier
    updated_time
  }
}
```

### 5.8 Xoa product

```graphql
mutation DeleteProduct {
  deleteProduct(id: "1")
}
```

## 6) Order

### 6.1 Tao order (Admin/Sale)

```graphql
mutation CreateOrder {
  createOrder(
    customer_id: "1"
    account_id: "1"
    shipping_address: "123 Demo Street"
    recipient_name: "Nguyen Van A"
    recipient_phone: "0900000001"
    recipient_email: "a@gmail.com"
    items: [
      { product_id: "1", quantity: 2 }
      { product_id: "2", quantity: 1 }
    ]
  ) {
    order_id
    created_time
    updated_time
    final_price
    status
    customer_id
    account_id
    shipping_address
    recipient_name
    recipient_phone
    recipient_email
    items {
      order_item_id
      product_id
      quantity
      unit_sale_price
      total_price
    }
  }
}
```

### 6.2 Danh sach order (Admin/Sale)

```graphql
query Orders {
  orders(page: 1, limit: 10) {
    data {
      order_id
      created_time
      final_price
      status
      recipient_name
      recipient_phone
      items {
        product_id
        quantity
        total_price
      }
    }
    total
    totalPages
  }
}
```

### 6.2.1 Lọc & Sắp xếp Order (Filter by Status, Date và Top Recent)

```graphql
query OrdersFiltered {
  orders(
    page: 1,
    limit: 10,
    filter: {
      status: "Processing",
      startDate: "2026-04-01",
      endDate: "2026-04-30"
    },
    sort: {
      field: CREATED_TIME,
      order: DESC
    }
  ) {
    data {
      order_id
      created_time
      final_price
      status
    }
    total
    totalPages
  }
}
```

### 6.3 Order theo id (Admin/Sale)

```graphql
query OrderById {
  order(id: "1") {
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
    items {
      order_item_id
      product_id
      quantity
      unit_sale_price
      total_price
    }
  }
}
```

### 6.4 Update đơn hàng FULL (Admin/Sale) - KHUYÊN DÙNG
Dùng để sửa địa chỉ, người nhận, status HOẶC thay đổi danh sách món đồ (Replace all items).

```graphql
mutation UpdateOrderFull {
  updateOrderFull(
    id: "1",
    input: {
      status: "Processing"
      shipping_address: "456 New Street, Ward 5"
      recipient_name: "Nguyen Van B"
      items: [
        { product_id: "1", quantity: 5 }
        { product_id: "3", quantity: 2 }
      ]
    }
  ) {
    order_id
    status
    final_price
    recipient_name
    shipping_address
    items {
      product_id
      quantity
      total_price
    }
  }
}
```

### 6.4.1 Cap nhat status order (Admin/Sale) - @DEPRECATED

```graphql
mutation UpdateOrderStatus {
  updateOrderStatus(id: "1", status: "Processing") {
    order_id
    status
    updated_time
  }
}
```

### 6.5 Soft Delete đơn hàng (Admin)
Lưu ý: Chỉ xóa được đơn ở trạng thái Created/Processing. Đơn đã đi giao (Shipped/Delivered) sẽ bị chặn.

```graphql
mutation DeleteOrder {
  deleteOrder(id: "1")
}
```

## 7) Report (Thống kê & Báo cáo)

***Lưu ý về tham số (Arguments) có thể truyền vào để query:***
- `period` (String): Khoảng thời gian gom nhóm dữ liệu. Các giá trị hợp lệ: `"day"`, `"week"`, `"month"`, `"year"`. (Mặc định: `"day"`).
- `startDate` (String): Ngày bắt đầu lọc dữ liệu (Định dạng: `"YYYY-MM-DD"`, ví dụ `"2026-01-01"`).
- `endDate` (String): Ngày kết thúc lọc dữ liệu (Định dạng: `"YYYY-MM-DD"`).
- `limit` (Int): Số lượng kết quả trả về, dùng cho Top Selling (Mặc định: `10`).

Tất cả các query Report đều yêu cầu quyền `Admin` hoặc `Sale` (cần Header `Authorization: Bearer <token>`).

### 7.1 Product sales report (Admin/Sale)

```graphql
query ProductSalesReport {
  productSalesReport(period: "day", startDate: "2026-01-01", endDate: "2026-12-31") {
    period
    date
    totalQuantity
    totalRevenue
    totalCost
    totalProfit
    products {
      product_id
      sku
      name
      quantity
      revenue
    }
  }
}
```

### 7.2 Revenue report (Admin/Sale)

```graphql
query RevenueReport {
  revenueReport(period: "month", startDate: "2026-01-01", endDate: "2026-12-31") {
    period
    date
    totalOrders
    totalRevenue
    totalItemsSold
    avgOrderValue
  }
}
```

### 7.3 Top selling products (phiên bản Report)
*Lưu ý: Query đã được đổi tên thành `topSellingProductsReport` để tránh trùng lặp với query cùng tên của module Product.*

```graphql
query TopSellingProductsReportModule {
  topSellingProductsReport(limit: 10, startDate: "2026-01-01", endDate: "2026-12-31") {
    product_id
    sku
    name
    price
    totalQuantity
    totalRevenue
    timesSold
  }
}
```

### 7.4 Sales overview (Admin/Sale)

```graphql
query SalesOverview {
  salesOverview(startDate: "2026-01-01", endDate: "2026-12-31") {
    totalOrders
    totalRevenue
    totalItemsSold
    uniqueCustomers
    avgOrderValue
    maxOrderValue
    minOrderValue
  }
}
```

## 8) Batch demo gợi ý (để trình bày nhanh)

1. Register Admin -> copy token.
2. Login -> copy token mới.
3. Thêm header `Authorization: Bearer <token>`.
4. Tạo category.
5. Tạo customer.
6. Tạo 2-3 product.
7. Tạo order (status ban đầu là Created), sau đó updateOrderFull -> Status: `Delivered` (Vì báo cáo chỉ tính đơn hàng Delivered).
8. Chạy các query report (Product Sales, Revenue, Top Selling, Overview) để show dashboard số liệu.
9. Chạy query `me` và `logout` để kết thúc demo auth.
