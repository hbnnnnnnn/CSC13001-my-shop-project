/*
 * GraphQL API — Products (MyShop backend)
 * Endpoint  : http://localhost:4000/graphql (configure via AppConfig:GraphQlEndpoint; docker-compose maps 4000:4000)
 * Auth      : None for product/category queries and product mutations (resolvers do not use requireAuth).
 *             JWT Bearer is parsed in context if present (category mutations require Admin role).
 * Server    : Apollo Server 4 + expressMiddleware on Express (see backend/src/server.js)
 * Transport : HTTP POST application/json { "query", "variables" }
 * ---
 * Schema    : backend/src/graphql/schema/productSchema.js, categorySchema.js
 * Price     : Int — DB seed uses whole currency units (e.g. VND dong as integer), not cents.
 * Images    : [String] URLs; REST upload POST /api/upload/ (multipart field "image") returns { imageUrl }
 */

using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace CSC13001_my_shop_project.Services;

public sealed class GraphQlClient
{
    private readonly HttpClient _http;
    private readonly ITokenService _tokenService;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public GraphQlClient(HttpClient http, ITokenService tokenService)
    {
        _http = http;
        _tokenService = tokenService;
    }

    private void AttachAuth()
    {
        var token = _tokenService.GetToken();
        if (!string.IsNullOrEmpty(token))
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        else
            _http.DefaultRequestHeaders.Authorization = null;
    }

    public async Task<T?> ExecuteAsync<T>(string document, object? variables = null, CancellationToken ct = default)
        where T : class
    {
        AttachAuth();

        var payload = new { query = document, variables };
        using var resp = await _http.PostAsJsonAsync("", payload, JsonOptions, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();

        var envelope = await resp.Content.ReadFromJsonAsync<GraphQlResponse<T>>(JsonOptions, ct).ConfigureAwait(false);
        if (envelope?.Errors is { Count: > 0 } errs)
            throw new GraphQlException(errs);

        return envelope?.Data;
    }
}
