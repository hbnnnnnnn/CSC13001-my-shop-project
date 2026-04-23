using System.Text.Json.Serialization;

namespace CSC13001_my_shop_project.Models;

public sealed record AIProductSuggestionDto
{
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }
}
