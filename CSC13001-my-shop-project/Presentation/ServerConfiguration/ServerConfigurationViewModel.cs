using System.Globalization;
using CommunityToolkit.Mvvm.Messaging;
using CSC13001_my_shop_project.Presentation.Dashboard;
using CSC13001_my_shop_project.Services;
using Windows.Storage;

namespace CSC13001_my_shop_project.Presentation.ServerConfiguration;

public partial class ServerConfigurationViewModel : ObservableObject
{
    private const string ServerUrlKey = "server_config.server_url";
    private const string PortKey = "server_config.port";
    private const string DatabaseKey = "server_config.database";
    private const string UsernameKey = "server_config.username";
    private const string PasswordKey = "server_config.password";

    private readonly INavigator _navigator;
    private readonly ApplicationDataContainer _localSettings;
    private readonly DbConfigService _dbConfigService;

    [ObservableProperty]
    private string serverUrl = "";

    [ObservableProperty]
    private string port = "";

    [ObservableProperty]
    private string database = "";

    [ObservableProperty]
    private string username = "";

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private bool isPasswordVisible;

    [ObservableProperty]
    private string errorMessage = string.Empty;
    [ObservableProperty]
    private string successMessage = string.Empty;

    [ObservableProperty]
    private bool isSaving;

    public ServerConfigurationViewModel(INavigator navigator, DbConfigService dbConfigService)
    {
        _navigator = navigator;
        _dbConfigService = dbConfigService;
        _localSettings = ApplicationData.Current.LocalSettings;
        BackToLogin = new AsyncRelayCommand(BackToLoginAsync);
        SaveConfiguration = new AsyncRelayCommand(SaveConfigurationAsync);
        Cancel = new AsyncRelayCommand(BackToLoginAsync);
        LoadSavedValues();
    }

    public IAsyncRelayCommand BackToLogin { get; }

    public IAsyncRelayCommand SaveConfiguration { get; }

    public IAsyncRelayCommand Cancel { get; }

    private void LoadSavedValues()
    {
        ServerUrl = ReadSetting(ServerUrlKey, ServerUrl);
        Port = ReadSetting(PortKey, Port);
        Database = ReadSetting(DatabaseKey, Database);
        Username = ReadSetting(UsernameKey, Username);
        Password = ReadSetting(PasswordKey, Password);
    }

    private string ReadSetting(string key, string fallback)
    {
        var value = _localSettings.Values[key] as string;
        return string.IsNullOrWhiteSpace(value) ? fallback : value;
    }

    private async Task BackToLoginAsync()
    {
        WeakReferenceMessenger.Default.Send(new NavigateToPageMessage("Login"));
        await Task.CompletedTask;
    }

    private async Task SaveConfigurationAsync()
    {
        if (IsSaving)
        {
            return;
        }

        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        var host = NormalizeHost(ServerUrl);
        if (string.IsNullOrWhiteSpace(host))
        {
            ErrorMessage = "Server URL is required.";
            return;
        }

        if (!int.TryParse(Port, NumberStyles.Integer, CultureInfo.InvariantCulture, out var portValue))
        {
            ErrorMessage = "Port must be a number.";
            return;
        }

        if (string.IsNullOrWhiteSpace(Database))
        {
            ErrorMessage = "Database name is required.";
            return;
        }

        if (string.IsNullOrWhiteSpace(Username))
        {
            ErrorMessage = "Username is required.";
            return;
        }

        IsSaving = true;
        try
        {
            await _dbConfigService.UpdateDbConfigAsync(
                new DbConfigRequest(
                    Host: host,
                    Port: portValue,
                    User: Username,
                    Password: Password ?? string.Empty,
                    Database: Database
                )
            );
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            return;
        }
        finally
        {
            IsSaving = false;
        }

        _localSettings.Values[ServerUrlKey] = ServerUrl;
        _localSettings.Values[PortKey] = Port;
        _localSettings.Values[DatabaseKey] = Database;
        _localSettings.Values[UsernameKey] = Username;
        _localSettings.Values[PasswordKey] = Password;
        SuccessMessage = "Database configuration updated successfully.";
    }

    private static string NormalizeHost(string value)
    {
        var trimmed = value?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return string.Empty;
        }

        if (Uri.TryCreate(trimmed, UriKind.Absolute, out var uri))
        {
            return uri.Host;
        }

        var host = trimmed;
        var slashIndex = host.IndexOf('/');
        if (slashIndex >= 0)
        {
            host = host.Substring(0, slashIndex);
        }

        var colonIndex = host.IndexOf(':');
        if (colonIndex >= 0)
        {
            host = host.Substring(0, colonIndex);
        }

        return host;
    }
}
