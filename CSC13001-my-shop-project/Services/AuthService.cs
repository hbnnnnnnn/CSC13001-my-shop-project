using System.Text.Json;
using CSC13001_my_shop_project.Models;

namespace CSC13001_my_shop_project.Services;

public class AuthService
{
    private readonly GraphqlService _graphql;
    private readonly ApplicationDataContainer _settings;

    private AccountModel? _currentAccount;
    private string? _token;

    public AuthService(GraphqlService graphql)
    {
        _graphql = graphql;
        _settings = ApplicationData.Current.LocalSettings;

        // Restore persisted token (from "Remember Me")
        _token = _settings.Values["auth_token"] as string;
    }

    /// <summary>Current JWT token, or null if not logged in.</summary>
    public string? Token => _token;

    /// <summary>Cached account info after login or me() call.</summary>
    public AccountModel? CurrentAccount => _currentAccount;

    /// <summary>Whether the user has an active token (may be expired).</summary>
    public bool IsLoggedIn => !string.IsNullOrEmpty(_token);

    /// <summary>
    /// Calls the <c>login</c> mutation, stores the token, and returns the account.
    /// </summary>
    public async Task<AccountModel> LoginAsync(string username, string password, bool rememberMe)
    {
        var data = await _graphql.QueryAsync(
            @"mutation Login($u: String!, $p: String!) {
                login(username: $u, password: $p) {
                    token
                    account { account_id username full_name account_role }
                }
            }",
            new { u = username, p = password }
        );

        var loginData = data.GetProperty("login");
        _token = loginData.GetProperty("token").GetString()!;
        _currentAccount = DeserializeAccount(loginData.GetProperty("account"));

        if (rememberMe)
            _settings.Values["auth_token"] = _token;
        else
            _settings.Values.Remove("auth_token");

        return _currentAccount;
    }

    /// <summary>
    /// Calls the <c>me</c> query to validate the stored token and fetch account info.
    /// Returns null if the token is invalid/expired.
    /// </summary>
    public async Task<AccountModel?> GetCurrentAccountAsync()
    {
        if (string.IsNullOrEmpty(_token))
        {
            Console.WriteLine("No token found");
            return null;
        }

        try
        {
            var data = await _graphql.QueryAsync(
                "query Me { me { account_id username full_name account_role } }"
            );

            _currentAccount = DeserializeAccount(data.GetProperty("me"));
            return _currentAccount;
        }
        catch (Exception ex)
        {
            // Token expired or invalid
            // ClearSession();
            Console.WriteLine($"Exception: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Calls the <c>logout</c> mutation (blacklists token server-side) and clears local state.
    /// </summary>
    public async Task LogoutAsync()
    {
        if (!string.IsNullOrEmpty(_token))
        {
            try
            {
                await _graphql.QueryAsync("mutation { logout }");
            }
            catch
            {
                /* best-effort — clear locally regardless */
            }
        }

        ClearSession();
    }

    private void ClearSession()
    {
        _token = null;
        _currentAccount = null;
        _settings.Values.Remove("auth_token");
    }

    private static AccountModel DeserializeAccount(JsonElement el) =>
        new(
            AccountId: el.GetProperty("account_id").GetString()!,
            Username: el.GetProperty("username").GetString()!,
            FullName: el.GetProperty("full_name").GetString()!,
            AccountRole: el.GetProperty("account_role").GetString()!
        );
}
