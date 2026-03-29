public class AppStateService
{
    private readonly ApplicationDataContainer _settings = ApplicationData.Current.LocalSettings;

    public string? LastPage
    {
        get => _settings.Values["last_page"] as string;
        set => _settings.Values["last_page"] = value;
    }

    public void Clear() => _settings.Values.Remove("last_page");
}
