using System.Text.Json.Serialization;

namespace CSC13001_my_shop_project.Models;

public record ProductDto
{
    [JsonPropertyName("product_id")]
    public string ProductId { get; init; } = "";

    public string Sku { get; init; } = "";

    public string Name { get; init; } = "";

    public int Price { get; init; }

    public int Stock { get; init; }

    public string? Description { get; init; }

    public List<string>? Images { get; init; }

    public string? Supplier { get; init; }

    public CategoryDto? Category { get; init; }

    [JsonPropertyName("created_time")]
    public string? CreatedTime { get; init; }

    [JsonPropertyName("updated_time")]
    public string? UpdatedTime { get; init; }
}
