using System.Net.Http.Headers;

namespace CSC13001_my_shop_project.Services.Endpoints;

/// <summary>
/// Attaches the Bearer token to every outgoing request.
/// Uses IServiceProvider to resolve AuthService lazily, avoiding a circular
/// dependency deadlock (AuthService → GraphqlService → HttpClient → AuthTokenHandler → AuthService).
/// </summary>
internal class AuthTokenHandler : DelegatingHandler
{
    private readonly IServiceProvider _sp;

    public AuthTokenHandler(IServiceProvider sp)
    {
        _sp = sp;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        var auth = _sp.GetRequiredService<AuthService>();

        if (!string.IsNullOrEmpty(auth.Token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
