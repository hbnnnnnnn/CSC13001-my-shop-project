using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CSC13001_my_shop_project.Services;

public class DbConfigService
{
  private readonly HttpClient _client;

  public DbConfigService(IHttpClientFactory httpClientFactory)
  {
    _client = httpClientFactory.CreateClient("BackendApi");
  }

  public async Task UpdateDbConfigAsync(DbConfigRequest request, CancellationToken ct = default)
  {
    var response = await _client.PostAsJsonAsync("/api/config/db", request, ct);
    if (response.IsSuccessStatusCode)
    {
      return;
    }

    var message = await TryReadErrorMessageAsync(response, ct);
    throw new InvalidOperationException(
      message ?? $"Failed to update database configuration ({(int)response.StatusCode})."
    );
  }

  private static async Task<string?> TryReadErrorMessageAsync(
    HttpResponseMessage response,
    CancellationToken ct
  )
  {
    try
    {
      var payload = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
      if (payload.ValueKind == JsonValueKind.Object)
      {
        if (payload.TryGetProperty("error", out var error))
          return error.GetString();
        if (payload.TryGetProperty("message", out var message))
          return message.GetString();
      }
    }
    catch
    {
      // Ignore payload parsing failures and fall back to status code.
    }

    return null;
  }
}

public sealed record DbConfigRequest(
  [property: JsonPropertyName("host")] string Host,
  [property: JsonPropertyName("port")] int Port,
  [property: JsonPropertyName("user")] string User,
  [property: JsonPropertyName("password")] string Password,
  [property: JsonPropertyName("database")] string Database
);