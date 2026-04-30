using System.Text.Json.Serialization;

namespace CSC13001_my_shop_project.Models;

public sealed class SalesOverviewDto
{
    [JsonPropertyName("totalOrders")]
    public int TotalOrders { get; set; }

    [JsonPropertyName("totalRevenue")]
    public int TotalRevenue { get; set; }

    [JsonPropertyName("totalItemsSold")]
    public int TotalItemsSold { get; set; }

    [JsonPropertyName("uniqueCustomers")]
    public int UniqueCustomers { get; set; }

    [JsonPropertyName("avgOrderValue")]
    public double AvgOrderValue { get; set; }

    [JsonPropertyName("maxOrderValue")]
    public int MaxOrderValue { get; set; }

    [JsonPropertyName("minOrderValue")]
    public int MinOrderValue { get; set; }
}
