using RESR.MAUI.Services;

namespace RESR.MAUI;

internal static class UserFeedback
{
    public const string NavigationError = "Impossible d'ouvrir cette page pour le moment.";
    public const string BackNavigationError = "Impossible de revenir a la page precedente pour le moment.";
    public const string TimeoutError = "Le serveur ne repond pas. Reessayez plus tard.";

    public static string FromApiException(ApiException ex, string fallbackMessage)
    {
        var message = DisplayText.ToExcerpt(ex.Message, 180);
        return string.IsNullOrWhiteSpace(message) ? fallbackMessage : message;
    }

    public static string FromTimeout(TimeoutException ex)
    {
        var message = DisplayText.ToExcerpt(ex.Message, 180);
        return string.IsNullOrWhiteSpace(message) ? TimeoutError : message;
    }

    public static string FromUnexpected(string fallbackMessage)
    {
        return fallbackMessage;
    }
}
