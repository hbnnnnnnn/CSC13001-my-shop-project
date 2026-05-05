using System.Text.Json.Serialization;
using CSC13001_my_shop_project.Models;

namespace CSC13001_my_shop_project.Services;

internal sealed record ProductsDataRoot([property: JsonPropertyName("products")] ProductPageDto? Products);

internal sealed record ProductDataRoot([property: JsonPropertyName("product")] ProductDto? Product);

internal sealed record CategoriesDataRoot([property: JsonPropertyName("categories")] CategoryListPayload? Categories);

internal sealed record CategoryListPayload(
    [property: JsonPropertyName("data")] List<CategoryDto>? Data,
    [property: JsonPropertyName("total")] int Total,
    [property: JsonPropertyName("page")] int Page,
    [property: JsonPropertyName("limit")] int Limit,
    [property: JsonPropertyName("totalPages")] int TotalPages
);

internal sealed record CreateProductDataRoot([property: JsonPropertyName("createProduct")] ProductDto? CreateProduct);

internal sealed record UpdateProductDataRoot([property: JsonPropertyName("updateProduct")] ProductDto? UpdateProduct);

internal sealed record DeleteProductDataRoot([property: JsonPropertyName("deleteProduct")] bool DeleteProduct);

internal sealed record CreateCategoryDataRoot([property: JsonPropertyName("createCategory")] CategoryDto? CreateCategory);

internal sealed record GenerateAiSuggestionDataRoot(
    [property: JsonPropertyName("generateProductDetailsFromImage")]
        AIProductSuggestionDto? GenerateProductDetailsFromImage);
