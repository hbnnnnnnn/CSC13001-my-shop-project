using System.Text.Json.Serialization;

namespace CSC13001_my_shop_project.Models;

public record CategoryDto
{
    [JsonPropertyName("category_id")]
    public string CategoryId { get; init; } = "";

    public string Name { get; init; } = "";

    public string? Description { get; init; }
}
