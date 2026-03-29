# Hướng dẫn chi tiết về Elasticsearch trong My Shop Project

Chào bạn! Đây là tài liệu chi tiết giải thích cách **Elasticsearch** được tích hợp và hoạt động trong hệ thống backend của bạn. Vì bạn mới bắt đầu tìm hiểu, mình sẽ giải thích từ những khái niệm cơ bản nhất đến cách mã nguồn hoạt động.

---

## 1. Elasticsearch là gì và tại sao chúng ta cần nó?

**Elasticsearch (ES)** là một công cụ tìm kiếm và phân tích dữ liệu siêu nhanh. 
- **Tại sao không dùng Database (PostgreSQL) để tìm kiếm?** Database truyền thống rất giỏi trong việc lưu trữ và truy vấn dữ liệu chính xác (ví dụ: tìm sản phẩm có ID là 10). Tuy nhiên, khi bạn muốn tìm kiếm "văn bản" (ví dụ: gõ "ao thun" nhưng muốn ra kết quả "Áo thun", "Áo phông", "Áo thun nam"), Database sẽ chạy rất chậm và kết quả không linh hoạt.
- **Lợi ích của ES:** Nó hỗ trợ "Full-text search" (tìm kiếm toàn văn), tìm kiếm mờ (fuzziness - gõ sai vẫn ra kết quả), và sắp xếp kết quả theo độ liên quan (relevancy).

Trong project này, chúng ta sử dụng ES để hỗ trợ tính năng **tìm kiếm sản phẩm** chuyên nghiệp.

---

## 2. Quy trình hoạt động (Data Flow)

Hệ thống của bạn hoạt động theo mô hình **"Dual-write"** (ghi vào hai nơi):
1. **Lưu trữ chính:** Dữ liệu vẫn được lưu vào PostgreSQL (Database chính) để đảm bảo an toàn.
2. **Đồng bộ:** Mỗi khi bạn Thêm/Sửa/Xóa sản phẩm ở Database, hệ thống sẽ tự động gửi một bản sao dữ liệu sang Elasticsearch.
3. **Truy vấn:** Khi người dùng sử dụng tính năng "Search", backend sẽ hỏi Elasticsearch thay vì hỏi PostgreSQL để lấy kết quả nhanh và chính xác nhất.

---

## 3. Chi tiết từng File và Mã nguồn

Chúng ta sẽ đi qua từng file liên quan theo thứ tự từ cài đặt đến thực thi.

### 3.1. `backend/docker-compose.yml` (Cài đặt môi trường)
Đây là nơi cấu hình để chạy server Elasticsearch.

```yaml
elasticsearch:
  image: elasticsearch:9.3.2 # Sử dụng phiên bản ES 9.3.2
  container_name: myshop_elasticsearch
  environment:
    - discovery.type=single-node # Chạy chế độ một máy (phù hợp cho dev)
    - xpack.security.enabled=false # Tắt bảo mật/mật khẩu để dễ code lúc đầu
    - ES_JAVA_OPTS=-Xms256m -Xmx256m # Giới hạn RAM (ES rất tốn RAM, 256MB là mức tối thiểu)
  ports:
    - "9200:9200" # Cổng kết nối (mặc định của ES)
```

### 3.2. `src/config/elasticsearch.js` (Kết nối)
File này dùng để khởi tạo "Client" (cầu nối) từ Node.js đến server ES.

```javascript
const { Client } = require('@elastic/elasticsearch');

const esClient = new Client({
  node: process.env.ELASTICSEARCH_NODE || 'http://localhost:9200', // Địa chỉ server ES
});

// Kiểm tra kết nối (ping)
esClient.ping()
  .then(() => console.log('Connected to Elasticsearch'))
  .catch((err) => console.error('Elasticsearch connection failed:', err.message));

module.exports = esClient;
```
- **Ý nghĩa:** Nó tạo ra một đối tượng `esClient` để các file khác có thể dùng để gửi yêu cầu (Search, Index...) tới ES.

### 3.3. `src/services/search.service.js` (Trái tim của logic ES)
Đây là file quan trọng nhất, chứa các "kỹ năng" tương tác với ES.

#### A. Hàm `createIndex`: Tạo cấu trúc tìm kiếm
ES không gọi là Table mà gọi là **Index**. Trước khi lưu dữ liệu, ta cần khai báo cấu trúc (Mapping).

```javascript
const createIndex = async () => {
  // ... kiểm tra nếu index đã tồn tại thì bỏ qua
  await esClient.indices.create({
    index: "products",
    body: {
      settings: {
        analysis: {
          analyzer: {
            product_analyzer: { // Bộ phân tích ngôn ngữ tùy chỉnh
              type: "custom",
              tokenizer: "standard",
              filter: ["lowercase", "asciifolding"], // "Áo" -> "ao" (giúp tìm kiếm không dấu)
            },
          },
        },
      },
      mappings: {
        properties: {
          product_id: { type: "integer" },
          name: { 
            type: "text", 
            analyzer: "product_analyzer", // Dùng bộ phân tích trên để tìm kiếm
            fields: { keyword: { type: "keyword" } } // Dùng để sắp xếp (Sort)
          },
          price: { type: "integer" },
          // ... các trường khác
        },
      },
    },
  });
};
```
- **Tại sao cần `asciifolding`?** Nó giúp biến các ký tự có dấu thành không dấu (ví dụ: `đ` -> `d`, `á` -> `a`). Nhờ đó người dùng gõ "dien thoai" vẫn tìm ra "điện thoại".

#### B. Các hàm đồng bộ dữ liệu (`indexProduct`, `indexUpdateProduct`, `indexDeleteProduct`)
Dùng để Thêm/Sửa/Xóa dữ liệu trong ES.
- `indexProduct`: Gửi toàn bộ thông tin 1 sản phẩm sang ES.
- `indexDeleteProduct`: Xóa sản phẩm khỏi bộ nhớ tìm kiếm bằng ID.

#### C. Hàm `searchProducts`: Thực hiện tìm kiếm
```javascript
const searchProducts = async (query, page = 1, limit = 10, filters = {}, sort = {}) => {
  // ... xây dựng các điều kiện lọc (filter) và sắp xếp (sort)
  const result = await esClient.search({
    index: "products",
    from: (page - 1) * limit, // Phân trang
    size: limit,
    query: {
      bool: {
        must: [
          {
            multi_match: { // Tìm kiếm trên nhiều trường cùng lúc
              query,
              fields: ["sku", "name^2", "description", "supplier", "category_name"],
              fuzziness: "AUTO", // Tìm kiếm mờ (sai vài ký tự vẫn ra)
            },
          },
        ],
        filter: filterClauses, // Lọc theo giá, danh mục...
      },
    },
  });
  // ... trả về dữ liệu định dạng dễ đọc
};
```
- **`name^2` là gì?** Đây gọi là "Boosting". Nó bảo ES rằng: "Nếu từ khóa nằm trong tên sản phẩm thì quan trọng gấp đôi so với nằm trong mô tả".

### 3.4. `src/scripts/syncElastic.js` (Đồng bộ lần đầu)
Khi bạn mới cài ES, nó sẽ trống rỗng. File này sẽ lấy toàn bộ sản phẩm từ PostgreSQL và đẩy sang ES.
- Lệnh chạy: `npm run es:sync` (đã được cấu hình trong `package.json`).

### 3.5. `src/services/product.service.js` (Kích hoạt đồng bộ)
Tại đây, chúng ta lồng code ES vào logic của Database.

```javascript
const createProduct = async (product) => {
    const newProduct = await productRepository.create(product); // Lưu vào DB
    const productWithCategory = await productRepository.findByIdWithCategory(newProduct.product_id);
    
    await indexProduct(productWithCategory); // THÊM VÀO ES NGAY LẬP TỨC
    
    return newProduct;
};
```
- **Tại sao làm vậy?** Để đảm bảo rằng ngay khi bạn vừa bấm "Lưu" sản phẩm mới, người dùng khác có thể tìm thấy nó ngay trên thanh tìm kiếm.

---

## 4. Cách sử dụng

1.  **Khởi động ES:** Chạy `docker-compose up -d`.
2.  **Đồng bộ dữ liệu ban đầu:** Chạy `npm run es:sync`. (Lưu ý: Trong `docker-compose.yml`, backend đã được cấu hình để tự chạy lệnh này khi khởi động).
3.  **Truy vấn qua GraphQL:** Bạn sử dụng Query `productSearch`.

**Ví dụ Query trong GraphQL:**
```graphql
query {
  productSearch(query: "iphone", page: 1, limit: 5) {
    data {
      name
      price
      category_name
    }
    total
  }
}
```

---

## 5. Tóm tắt các thuật ngữ cho người mới
- **Index:** Giống như một Database hoặc Table trong SQL, nhưng dành cho ES.
- **Document:** Một bản ghi dữ liệu (tương ứng với 1 hàng trong SQL).
- **Mapping:** Khai báo kiểu dữ liệu cho các trường (giống Schema).
- **Analyzer:** Bộ máy xử lý văn bản (cắt từ, chuyển chữ thường, bỏ dấu).
- **Fuzziness:** Khả năng chịu lỗi khi người dùng gõ sai chính tả.

Hy vọng tài liệu này giúp bạn hiểu rõ "bức tranh" về Elasticsearch trong dự án của mình! Nếu có chỗ nào chưa rõ, cứ hỏi mình nhé.
