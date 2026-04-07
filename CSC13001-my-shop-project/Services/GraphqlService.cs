using System.Net.Http.Json;
using System.Text.Json;

namespace CSC13001_my_shop_project.Services;

public class GraphqlService
{
    private readonly HttpClient _client;

    public GraphqlService(IHttpClientFactory httpClientFactory)
    {
        _client = httpClientFactory.CreateClient("BackendApi");
    }

    public async Task<JsonElement> QueryAsync(string query, object? variables = null)
    {
        var payload = new { query, variables };

        var response = await _client.PostAsJsonAsync("/graphql", payload);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<JsonElement>();

        if (result.TryGetProperty("errors", out var errors))
        {
            var message = errors[0].GetProperty("message").GetString();
            throw new GraphqlException(message ?? "Unknown GraphQL error");
        }

        return result.GetProperty("data");
    }
}

public class GraphqlException : Exception
{
    public GraphqlException(string message)
        : base(message) { }
}
