using CSC13001_my_shop_project.Models;

namespace CSC13001_my_shop_project.Services;

public interface IProductService
{
    Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync();

    Task<CreatedProductSummary> CreateAsync(CreateProductInput input);
}
