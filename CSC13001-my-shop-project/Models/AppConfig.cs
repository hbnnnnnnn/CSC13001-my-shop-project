namespace CSC13001_my_shop_project.Models;

public record AppConfig
{
    public string? Environment { get; init; }

    /// <summary>GraphQL HTTP endpoint (POST), e.g. http://localhost:4000/graphql</summary>
    public string? GraphQlEndpoint { get; init; }
}
