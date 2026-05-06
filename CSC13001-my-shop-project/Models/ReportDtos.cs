using System.Text.Json.Serialization;

namespace CSC13001_my_shop_project.Models;

public sealed class ProductSalesPeriodDto
{
    public string Period { get; set; } = "";

    [JsonConverter(typeof(ReportDateStringConverter))]
    public string Date { get; set; } = "";

    [JsonPropertyName("totalQuantity")]
    public int TotalQuantity { get; set; }

    [JsonPropertyName("totalRevenue")]
    public int TotalRevenue { get; set; }

    [JsonPropertyName("totalCost")]
    public int TotalCost { get; set; }

    [JsonPropertyName("totalProfit")]
    public int TotalProfit { get; set; }

    public List<ProductSalesLineDto> Products { get; set; } = [];
}

public sealed class ProductSalesLineDto
{
    [JsonPropertyName("product_id")]
    [JsonConverter(typeof(GraphQlIdStringConverter))]
    public string ProductId { get; set; } = "";

    public string Sku { get; set; } = "";

    public string Name { get; set; } = "";

    public int Quantity { get; set; }

    public int Revenue { get; set; }

    [JsonPropertyName("totalCost")]
    public int TotalCost { get; set; }

    [JsonPropertyName("totalProfit")]
    public int TotalProfit { get; set; }
}

public sealed class CategorySalesPeriodDto
{
    public string Period { get; set; } = "";

    [JsonConverter(typeof(ReportDateStringConverter))]
    public string Date { get; set; } = "";

    [JsonPropertyName("totalQuantity")]
    public int TotalQuantity { get; set; }

    [JsonPropertyName("totalRevenue")]
    public int TotalRevenue { get; set; }

    [JsonPropertyName("totalCost")]
    public int TotalCost { get; set; }

    [JsonPropertyName("totalProfit")]
    public int TotalProfit { get; set; }

    public List<CategorySalesLineDto> Categories { get; set; } = [];
}

public sealed class CategorySalesLineDto
{
    [JsonPropertyName("category_id")]
    [JsonConverter(typeof(GraphQlIdStringConverter))]
    public string CategoryId { get; set; } = "";

    [JsonPropertyName("category_name")]
    public string CategoryName { get; set; } = "";

    public int Quantity { get; set; }

    public int Revenue { get; set; }

    [JsonPropertyName("totalCost")]
    public int TotalCost { get; set; }

    [JsonPropertyName("totalProfit")]
    public int TotalProfit { get; set; }
}

public sealed class RevenuePeriodDto
{
    public string Period { get; set; } = "";

    [JsonConverter(typeof(ReportDateStringConverter))]
    public string Date { get; set; } = "";

    [JsonPropertyName("totalOrders")]
    public int TotalOrders { get; set; }

    [JsonPropertyName("totalRevenue")]
    public int TotalRevenue { get; set; }

    [JsonPropertyName("totalCost")]
    public int TotalCost { get; set; }

    [JsonPropertyName("totalProfit")]
    public int TotalProfit { get; set; }

    [JsonPropertyName("totalItemsSold")]
    public int TotalItemsSold { get; set; }

    [JsonPropertyName("avgOrderValue")]
    public double AvgOrderValue { get; set; }
}

public sealed class TopSellingProductDto
{
    [JsonPropertyName("product_id")]
    [JsonConverter(typeof(GraphQlIdStringConverter))]
    public string ProductId { get; set; } = "";

    public string Sku { get; set; } = "";

    public string Name { get; set; } = "";

    public int Price { get; set; }

    [JsonPropertyName("totalQuantity")]
    public int TotalQuantity { get; set; }

    [JsonPropertyName("totalRevenue")]
    public int TotalRevenue { get; set; }

    [JsonPropertyName("timesSold")]
    public int TimesSold { get; set; }
}
