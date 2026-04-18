using CSC13001_my_shop_project.Models;

namespace CSC13001_my_shop_project.Services;

public interface IProductService
{
    Task<ProductPageDto> GetProductsAsync(
        int page = 1,
        int limit = 20,
        ProductFilterInput? filter = null,
        ProductSortInput? sort = null,
        CancellationToken ct = default);

    Task<ProductDto?> GetByIdAsync(string id, CancellationToken ct = default);

    Task<ProductDto> CreateAsync(CreateProductInput input, CancellationToken ct = default);

    Task<ProductDto> UpdateAsync(string id, UpdateProductInput input, CancellationToken ct = default);

    Task<bool> DeleteAsync(string id, CancellationToken ct = default);

    Task<List<CategoryDto>> GetCategoriesAsync(CancellationToken ct = default);
}
