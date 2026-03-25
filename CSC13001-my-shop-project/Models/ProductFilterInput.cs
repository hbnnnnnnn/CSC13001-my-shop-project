using System.Text.Json.Serialization;

namespace CSC13001_my_shop_project.Models;

public record ProductFilterInput
{
    [JsonPropertyName("category_id")]
    public string? CategoryId { get; init; }

    [JsonPropertyName("min_price")]
    public int? MinPrice { get; init; }

    [JsonPropertyName("max_price")]
    public int? MaxPrice { get; init; }
}
