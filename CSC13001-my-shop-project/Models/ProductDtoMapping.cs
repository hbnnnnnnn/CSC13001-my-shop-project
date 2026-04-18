using System.Globalization;

namespace CSC13001_my_shop_project.Models;

public static class ProductDtoMapping
{
    public static ProductListItem ToListItem(ProductDto p)
    {
        var id = int.TryParse(p.ProductId, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n)
            ? n
            : 0;

        DateTime? created = null;
        if (!string.IsNullOrWhiteSpace(p.CreatedTime)
            && DateTime.TryParse(
                p.CreatedTime,
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind,
                out var parsed))
        {
            created = parsed;
        }

        var status = p.Stock <= 0
            ? ProductShelfStatus.OutOfStock
            : p.Stock < 10
                ? ProductShelfStatus.LowStock
                : ProductShelfStatus.Active;

        var image = p.Images is { Count: > 0 } ? p.Images[0] : "";
        var category = p.Category?.Name ?? "";

        return new ProductListItem(
            id,
            p.Name,
            category,
            p.Sku,
            image,
            0,
            0,
            p.Price,
            null,
            p.Stock,
            status,
            created,
            string.IsNullOrEmpty(p.ProductId) ? null : p.ProductId
        );
    }
}
