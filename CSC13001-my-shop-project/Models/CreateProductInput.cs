using System.Text.Json.Serialization;

namespace CSC13001_my_shop_project.Models;

public record CreateProductInput
{
    public string Sku { get; init; } = "";

    public string Name { get; init; } = "";

    public int Price { get; init; }

    public int Stock { get; init; }

    public string? Description { get; init; }

    public List<string>? Images { get; init; }

    public string? Supplier { get; init; }

    [JsonPropertyName("category_id")]
    public string CategoryId { get; init; } = "";
}
