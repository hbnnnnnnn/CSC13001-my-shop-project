using System.Text.Json.Serialization;

namespace CSC13001_my_shop_project.Models;

public record UpdateProductInput
{
    [JsonPropertyName("sku")]
    public string? Sku { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("price")]
    public int? Price { get; init; }

    [JsonPropertyName("stock")]
    public int? Stock { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("images")]
    public List<string>? Images { get; init; }

    [JsonPropertyName("supplier")]
    public string? Supplier { get; init; }

    [JsonPropertyName("category_id")]
    public string? CategoryId { get; init; }
}
