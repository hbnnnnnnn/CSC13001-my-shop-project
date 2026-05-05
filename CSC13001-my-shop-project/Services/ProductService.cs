using CSC13001_my_shop_project.Models;

namespace CSC13001_my_shop_project.Services;

public sealed class ProductService(GraphQlClient gql) : IProductService
{
    public async Task<ProductPageDto> GetProductsAsync(
        int page = 1,
        int limit = 20,
        ProductFilterInput? filter = null,
        ProductSortInput? sort = null,
        CancellationToken ct = default)
    {
        var root = await gql.ExecuteAsync<ProductsDataRoot>(
            ProductDocuments.GetProducts,
            new { page, limit, filter, sort },
            ct).ConfigureAwait(false);

        return root?.Products
            ?? new ProductPageDto { Data = [], Total = 0, Page = page, Limit = limit, TotalPages = 0 };
    }

    public async Task<ProductDto?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        var root = await gql.ExecuteAsync<ProductDataRoot>(
            ProductDocuments.GetProduct,
            new { id },
            ct).ConfigureAwait(false);
        return root?.Product;
    }

    public async Task<ProductDto> CreateAsync(CreateProductInput input, CancellationToken ct = default)
    {
        var root = await gql.ExecuteAsync<CreateProductDataRoot>(
            ProductDocuments.CreateProduct,
            new { input },
            ct).ConfigureAwait(false);
        var p = root?.CreateProduct ?? throw new InvalidOperationException("createProduct returned no data.");
        return p;
    }

    public async Task<ProductDto> UpdateAsync(string id, UpdateProductInput input, CancellationToken ct = default)
    {
        var root = await gql.ExecuteAsync<UpdateProductDataRoot>(
            ProductDocuments.UpdateProduct,
            new { id, input },
            ct).ConfigureAwait(false);
        var p = root?.UpdateProduct ?? throw new InvalidOperationException("updateProduct returned no data.");
        return p;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken ct = default)
    {
        var root = await gql.ExecuteAsync<DeleteProductDataRoot>(
            ProductDocuments.DeleteProduct,
            new { id },
            ct).ConfigureAwait(false);
        return root?.DeleteProduct ?? false;
    }

    public async Task<List<CategoryDto>> GetCategoriesAsync(CancellationToken ct = default)
    {
        var root = await gql.ExecuteAsync<CategoriesDataRoot>(
            ProductDocuments.GetCategories,
            new { page = 1, limit = 200 },
            ct).ConfigureAwait(false);
        return root?.Categories?.Data ?? [];
    }

    public async Task<CategoryDto> CreateCategoryAsync(
        string name,
        string? description,
        CancellationToken ct = default)
    {
        var root = await gql.ExecuteAsync<CreateCategoryDataRoot>(
            ProductDocuments.CreateCategory,
            new { name, description },
            ct).ConfigureAwait(false);
        return root?.CreateCategory
            ?? throw new InvalidOperationException("createCategory returned no data.");
    }

    public async Task<AIProductSuggestionDto?> GenerateProductDetailsFromImageAsync(
        string imageUrl,
        CancellationToken ct = default)
    {
        var root = await gql
            .ExecuteAsync<GenerateAiSuggestionDataRoot>(
                ProductDocuments.GenerateProductDetailsFromImage,
                new { imageUrl },
                ct)
            .ConfigureAwait(false);
        return root?.GenerateProductDetailsFromImage;
    }
}
