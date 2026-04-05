using CommunityToolkit.Mvvm.Messaging;
using CSC13001_my_shop_project.Presentation.Dashboard;
using Windows.Storage;

namespace CSC13001_my_shop_project.Presentation.ServerConfiguration;

public partial class ServerConfigurationViewModel : ObservableObject
{
    private const string ServerUrlKey = "ServerConfig.ServerUrl";
    private const string PortKey = "ServerConfig.Port";
    private const string DatabaseKey = "ServerConfig.Database";
    private const string UsernameKey = "ServerConfig.Username";
    private const string PasswordKey = "ServerConfig.Password";
    private const string EnableSslKey = "ServerConfig.EnableSsl";

    private readonly INavigator _navigator;
    private readonly ApplicationDataContainer _localSettings;

    [ObservableProperty]
    private string serverUrl = "http://localhost";

    [ObservableProperty]
    private string port = "5432";

    [ObservableProperty]
    private string database = "luminahaven_db";

    [ObservableProperty]
    private string username = "admin";

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private bool isPasswordVisible;

    [ObservableProperty]
    private bool enableSsl = true;

    public ServerConfigurationViewModel(INavigator navigator)
    {
        _navigator = navigator;
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
        EnableSsl = ReadBoolSetting(EnableSslKey, EnableSsl);
    }

    private string ReadSetting(string key, string fallback)
    {
        var value = _localSettings.Values[key] as string;
        return string.IsNullOrWhiteSpace(value) ? fallback : value;
    }

    private bool ReadBoolSetting(string key, bool fallback)
    {
        if (_localSettings.Values[key] is bool value)
            return value;

        return fallback;
    }

    private async Task BackToLoginAsync()
    {
        WeakReferenceMessenger.Default.Send(new NavigateToPageMessage("Login"));
        await Task.CompletedTask;
    }

    private async Task SaveConfigurationAsync()
    {
        _localSettings.Values[ServerUrlKey] = ServerUrl;
        _localSettings.Values[PortKey] = Port;
        _localSettings.Values[DatabaseKey] = Database;
        _localSettings.Values[UsernameKey] = Username;
        _localSettings.Values[PasswordKey] = Password;
        _localSettings.Values[EnableSslKey] = EnableSsl;

        await BackToLoginAsync();
    }
}
