using System.Text.Json;
using CSC13001_my_shop_project.Models;

namespace CSC13001_my_shop_project.Services;

public sealed class ProductService : IProductService
{
    private readonly GraphqlService _graphql;

    public ProductService(GraphqlService graphql)
    {
        _graphql = graphql;
    }

    public async Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync()
    {
        const string query =
            @"query { categories(page: 1, limit: 500) { data { category_id name description } } }";

        var data = await _graphql.QueryAsync(query);
        var list = data.GetProperty("categories").GetProperty("data");
        var result = new List<CategoryDto>();
        foreach (var row in list.EnumerateArray())
        {
            var id = row.GetProperty("category_id").ToString();
            var name = row.GetProperty("name").GetString() ?? "";
            string? desc = null;
            if (row.TryGetProperty("description", out var d) && d.ValueKind != JsonValueKind.Null)
                desc = d.GetString();

            result.Add(
                new CategoryDto
                {
                    CategoryId = id,
                    Name = name,
                    Description = desc,
                }
            );
        }

        return result;
    }

    public async Task<CreatedProductSummary> CreateAsync(CreateProductInput input)
    {
        const string mutation =
            @"mutation($input: CreateProductInput!) {
                createProduct(input: $input) {
                    product_id sku name price stock
                    images
                    category { name }
                }
            }";

        var inner = new Dictionary<string, object?>
        {
            ["sku"] = input.Sku,
            ["name"] = input.Name,
            ["price"] = input.Price,
            ["stock"] = input.Stock,
            ["category_id"] = input.CategoryId,
        };
        if (!string.IsNullOrWhiteSpace(input.Description))
            inner["description"] = input.Description;
        if (input.Images is { Count: > 0 })
            inner["images"] = input.Images;
        if (!string.IsNullOrWhiteSpace(input.Supplier))
            inner["supplier"] = input.Supplier;

        var variables = new Dictionary<string, object?> { ["input"] = inner };

        var data = await _graphql.QueryAsync(mutation, variables);
        var p = data.GetProperty("createProduct");

        var id = int.Parse(p.GetProperty("product_id").ToString());
        var sku = p.GetProperty("sku").GetString() ?? "";
        var name = p.GetProperty("name").GetString() ?? "";
        var price = p.GetProperty("price").GetInt32();
        var stock = p.GetProperty("stock").GetInt32();
        var categoryName = p.GetProperty("category").GetProperty("name").GetString() ?? "";

        string? firstImg = null;
        if (p.TryGetProperty("images", out var imgs) && imgs.ValueKind == JsonValueKind.Array)
        {
            foreach (var el in imgs.EnumerateArray())
            {
                var s = el.GetString();
                if (!string.IsNullOrEmpty(s))
                {
                    firstImg = s;
                    break;
                }
            }
        }

        return new CreatedProductSummary
        {
            Id = id,
            Sku = sku,
            Name = name,
            CategoryName = categoryName,
            Price = price,
            Stock = stock,
            FirstImageUrl = firstImg,
        };
    }
}
