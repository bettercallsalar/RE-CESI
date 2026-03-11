namespace RESR.MAUI.Services;

public sealed class ApiSession : IApiSession
{
    public string? Token { get; set; }

    public void Clear()
    {
        Token = null;
    }
}
