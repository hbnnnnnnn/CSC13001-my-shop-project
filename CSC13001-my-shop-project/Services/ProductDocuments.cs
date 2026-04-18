namespace CSC13001_my_shop_project.Services;

public static class ProductDocuments
{
    public const string GetProducts = """
        query GetProducts($page: Int, $limit: Int, $filter: ProductFilter, $sort: ProductSort) {
          products(page: $page, limit: $limit, filter: $filter, sort: $sort) {
            data {
              product_id
              sku
              name
              price
              stock
              description
              images
              supplier
              category { category_id name }
              created_time
              updated_time
            }
            total
            page
            limit
            totalPages
          }
        }
        """;

    public const string GetProduct = """
        query GetProduct($id: ID!) {
          product(id: $id) {
            product_id
            sku
            name
            price
            stock
            description
            images
            supplier
            category { category_id name description }
            created_time
            updated_time
          }
        }
        """;

    public const string GetCategories = """
        query GetCategories($page: Int, $limit: Int) {
          categories(page: $page, limit: $limit) {
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
        """;

    public const string CreateProduct = """
        mutation CreateProduct($input: CreateProductInput!) {
          createProduct(input: $input) {
            product_id
            sku
            name
            price
            stock
            description
            images
            supplier
            category { category_id name }
            created_time
            updated_time
          }
        }
        """;

    public const string UpdateProduct = """
        mutation UpdateProduct($id: ID!, $input: UpdateProductInput!) {
          updateProduct(id: $id, input: $input) {
            product_id
            sku
            name
            price
            stock
            description
            images
            supplier
            category { category_id name }
            updated_time
          }
        }
        """;

    public const string DeleteProduct = """
        mutation DeleteProduct($id: ID!) {
          deleteProduct(id: $id)
        }
        """;
}
