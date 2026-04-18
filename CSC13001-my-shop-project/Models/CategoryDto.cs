namespace CSC13001_my_shop_project.Models;

public sealed class CategoryDto
{
    public string CategoryId { get; init; } = "";

    public string Name { get; init; } = "";

    public string? Description { get; init; }

    public override string ToString() => Name;
}
