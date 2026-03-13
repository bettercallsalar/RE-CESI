namespace RESR.MAUI.Services;

public sealed class ApiSession : IApiSession
{
    private const string TokenKey = "auth_token";
    private string? _token;

    public ApiSession()
    {
        _token = PreferencesShim.Get(TokenKey, null);
    }

    public string? Token
    {
        get => _token;
        set
        {
            _token = value;

            if (string.IsNullOrWhiteSpace(value))
            {
                PreferencesShim.Remove(TokenKey);
            }
            else
            {
                PreferencesShim.Set(TokenKey, value);
            }
        }
    }

    public void Clear()
    {
        Token = null;
    }
}

internal static class PreferencesShim
{
#if ANDROID || IOS || MACCATALYST || WINDOWS || TIZEN || MAUI
    public static string? Get(string key, string? defaultValue)
        => Microsoft.Maui.Storage.Preferences.Get(key, defaultValue);

    public static void Set(string key, string value)
        => Microsoft.Maui.Storage.Preferences.Set(key, value);

    public static void Remove(string key)
        => Microsoft.Maui.Storage.Preferences.Remove(key);
#else
    private static readonly Dictionary<string, string?> Store = new();

    public static string? Get(string key, string? defaultValue)
        => Store.TryGetValue(key, out var value) ? value : defaultValue;

    public static void Set(string key, string value)
        => Store[key] = value;

    public static void Remove(string key)
        => Store.Remove(key);
#endif
}
