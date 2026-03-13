using Microsoft.Maui.Storage;

namespace RESR.MAUI.Services;

public sealed class ApiSession : IApiSession
{
    private const string TokenKey = "auth_token";
    private string? _token;

    public ApiSession()
    {
        _token = Preferences.Get(TokenKey, null);
    }

    public string? Token
    {
        get => _token;
        set
        {
            _token = value;

            if (string.IsNullOrWhiteSpace(value))
            {
                Preferences.Remove(TokenKey);
            }
            else
            {
                Preferences.Set(TokenKey, value);
            }
        }
    }

    public void Clear()
    {
        Token = null;
    }
}
