using System.Linq;
using System.Text.Json.Serialization;

namespace CSC13001_my_shop_project.Services;

public sealed class GraphQlException : Exception
{
    public IReadOnlyList<GraphQlError> Errors { get; }

    public GraphQlException(IReadOnlyList<GraphQlError> errors)
        : base(string.Join("; ", errors.Select(e => e.Message)))
    {
        Errors = errors;
    }
}

public sealed record GraphQlError(
    string Message,
    List<GraphQlErrorLocation>? Locations,
    List<object>? Path
);

public sealed record GraphQlErrorLocation(int Line, int Column);

public sealed class GraphQlResponse<T>
{
    [JsonPropertyName("data")]
    public T? Data { get; set; }

    [JsonPropertyName("errors")]
    public List<GraphQlError>? Errors { get; set; }
}
