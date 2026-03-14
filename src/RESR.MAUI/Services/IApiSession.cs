namespace RESR.MAUI.Services;

public interface IApiSession
{
    string? Token { get; set; }
    bool IsAuthenticated => !string.IsNullOrWhiteSpace(Token);
    void Clear();
}
