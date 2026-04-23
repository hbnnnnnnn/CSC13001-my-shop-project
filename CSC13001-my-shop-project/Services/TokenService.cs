namespace CSC13001_my_shop_project.Services;

/// <summary>Forwards the JWT from <see cref="AuthService"/> so <see cref="GraphQlClient"/> sends the same Bearer token as <c>BackendApi</c>.</summary>
public sealed class TokenService(AuthService auth) : ITokenService
{
    public string? GetToken() => auth.Token;
}
