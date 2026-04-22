using System.Text.Json.Serialization;

namespace CSC13001_my_shop_project.Models;

public record ProductPageDto
{
    [JsonPropertyName("data")]
    public List<ProductDto> Data { get; init; } = [];

    [JsonPropertyName("total")]
    public int Total { get; init; }

    [JsonPropertyName("page")]
    public int Page { get; init; }

    [JsonPropertyName("limit")]
    public int Limit { get; init; }

    [JsonPropertyName("totalPages")]
    public int TotalPages { get; init; }
}
