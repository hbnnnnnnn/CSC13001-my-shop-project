namespace CSC13001_my_shop_project.Models;

/// <summary>Sample catalog shared by products list and product detail.</summary>
public static class ProductCatalogData
{
    public static IReadOnlyList<ProductListItem> Items { get; } = BuildItems();

    public static ProductListItem? FindById(int id) => Items.FirstOrDefault(p => p.Id == id);

    private static IReadOnlyList<ProductListItem> BuildItems()
    {
        static string PickImage(int index) =>
            (index % 6) switch
            {
                0 => "ms-appx:///Assets/Products/woven-basket.png",
                1 => "ms-appx:///Assets/Products/teak-coffee-table.png",
                2 => "ms-appx:///Assets/Products/bamboo-floor-lamp.png",
                3 => "ms-appx:///Assets/Products/ceramic-planter.png",
                4 => "ms-appx:///Assets/Products/beige-modular-sofa.png",
                _ => "ms-appx:///Assets/Products/woven-basket.png",
            };

        return
        [
            new(
                12,
                "Woven Storage Basket",
                "Storage",
                "WSB-010",
                PickImage(0),
                4.5,
                48,
                38m,
                150m,
                60,
                ProductShelfStatus.Active
            ),
            new(
                11,
                "Teak Coffee Table",
                "Furniture",
                "TCT-204",
                PickImage(1),
                4.8,
                112,
                249m,
                null,
                8,
                ProductShelfStatus.LowStock
            ),
            new(
                10,
                "Bamboo Floor Lamp",
                "Lighting",
                "BFL-088",
                PickImage(2),
                4.2,
                36,
                89m,
                120m,
                0,
                ProductShelfStatus.OutOfStock
            ),
            new(
                9,
                "Ceramic Planter Set",
                "Decor",
                "CPS-331",
                PickImage(3),
                4.6,
                67,
                45m,
                null,
                24,
                ProductShelfStatus.Active
            ),
            new(
                8,
                "Beige Modular Sofa",
                "Furniture",
                "BMS-901",
                PickImage(4),
                4.9,
                203,
                899m,
                1099m,
                3,
                ProductShelfStatus.LowStock
            ),
            new(
                7,
                "Linen Woven Rug",
                "Textiles",
                "LWR-445",
                PickImage(5),
                4.1,
                19,
                120m,
                null,
                6,
                ProductShelfStatus.LowStock
            ),
            new(
                6,
                "Nordic Lounge Chair",
                "Furniture",
                "NLC-112",
                PickImage(1),
                4.7,
                89,
                249m,
                299m,
                0,
                ProductShelfStatus.OutOfStock
            ),
            new(
                5,
                "Minimalist Wooden Lamp",
                "Lighting",
                "MWL-077",
                PickImage(2),
                4.0,
                54,
                79m,
                null,
                42,
                ProductShelfStatus.Active
            ),
            new(
                4,
                "Oak Side Table",
                "Furniture",
                "OST-556",
                PickImage(1),
                4.3,
                31,
                129m,
                159m,
                4,
                ProductShelfStatus.LowStock
            ),
            new(
                3,
                "Glass Vase Trio",
                "Decor",
                "GVT-223",
                PickImage(3),
                4.4,
                22,
                55m,
                null,
                11,
                ProductShelfStatus.Active
            ),
            new(
                2,
                "Cotton Throw Blanket",
                "Textiles",
                "CTB-668",
                PickImage(5),
                4.8,
                140,
                68m,
                85m,
                2,
                ProductShelfStatus.LowStock
            ),
            new(
                1,
                "Ceramic Artisan Vase",
                "Decor",
                "CAV-019",
                PickImage(3),
                4.5,
                98,
                45m,
                null,
                30,
                ProductShelfStatus.Active
            ),
        ];
    }
}
