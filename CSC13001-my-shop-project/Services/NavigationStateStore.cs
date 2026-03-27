namespace CSC13001_my_shop_project.Services;

/// <summary>
/// In-memory store for page-level navigation state.
/// Registered as a singleton so state survives across page transitions.
/// </summary>
public sealed class NavigationStateStore
{
    private readonly Dictionary<string, object> _cache = new();

    public void Save<T>(string key, T state)
        where T : class => _cache[key] = state;

    public T? Restore<T>(string key)
        where T : class => _cache.TryGetValue(key, out var value) ? value as T : null;

    public void Clear(string key) => _cache.Remove(key);
}
