using System.Text.Json.Serialization;

namespace CSC13001_my_shop_project.Models;

/// <summary>GraphQL input <c>ProductSort</c> — JSON must use camelCase <c>field</c> / <c>order</c>.</summary>
public record ProductSortInput(
    [property: JsonPropertyName("field")] string Field,
    [property: JsonPropertyName("order")] string Order);
