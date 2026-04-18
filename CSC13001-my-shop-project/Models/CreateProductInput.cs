namespace CSC13001_my_shop_project.Models;

public sealed class CreateProductInput
{
    public string Sku { get; set; } = "";

    public string Name { get; set; } = "";

    public int Price { get; set; }

    public int Stock { get; set; }

    public string? Description { get; set; }

    public IReadOnlyList<string>? Images { get; set; }

    public string? Supplier { get; set; }

    public string CategoryId { get; set; } = "";
}
