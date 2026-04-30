using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CSC13001_my_shop_project.Models;

/// <summary>GraphQL/JSON có thể trả date dạng chuỗi ISO hoặc kiểu khác — ép về chuỗi để sort/nhãn.</summary>
public sealed class ReportDateStringConverter : JsonConverter<string>
{
    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                return reader.GetString() ?? "";
            case JsonTokenType.Null:
                return "";
            case JsonTokenType.Number:
                return reader.GetDouble().ToString(CultureInfo.InvariantCulture);
            default:
            {
                using var doc = JsonDocument.ParseValue(ref reader);
                return doc.RootElement.ValueKind switch
                {
                    JsonValueKind.String => doc.RootElement.GetString() ?? "",
                    _ => doc.RootElement.GetRawText(),
                };
            }
        }
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value);
}

/// <summary>ID GraphQL đôi khi là số trong JSON — map sang string.</summary>
public sealed class GraphQlIdStringConverter : JsonConverter<string>
{
    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                return reader.GetString() ?? "";
            case JsonTokenType.Number:
                return reader.TryGetInt64(out var n)
                    ? n.ToString(CultureInfo.InvariantCulture)
                    : reader.GetDouble().ToString(CultureInfo.InvariantCulture);
            case JsonTokenType.Null:
                return "";
            default:
                return "";
        }
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value);
}
