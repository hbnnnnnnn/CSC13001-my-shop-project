namespace CSC13001_my_shop_project.Services;

public sealed class CreatedProductSummary
{
    public int Id { get; init; }

    public string Sku { get; init; } = "";

    public string Name { get; init; } = "";

    public string CategoryName { get; init; } = "";

    public int Price { get; init; }

    public int Stock { get; init; }

    public string? FirstImageUrl { get; init; }
}
