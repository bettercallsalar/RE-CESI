using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace RESR.MAUI.Services;

internal static partial class ApiClientErrors
{
    private const string DefaultErrorMessage = "Une erreur est survenue. Reessayez plus tard.";
    private const string TimeoutMessage = "Le serveur ne repond pas. Reessayez plus tard.";

    public static async Task<ApiException> FromResponseAsync(HttpResponseMessage response, CancellationToken ct)
    {
        var content = await response.Content.ReadAsStringAsync(ct);
        var message = ExtractMessage(content);

        if (string.IsNullOrWhiteSpace(message))
            message = BuildDefaultMessage(response.StatusCode);

        return new ApiException(response.StatusCode, message);
    }

    public static ApiException InvalidResponse(HttpStatusCode statusCode, string message)
    {
        return new ApiException(statusCode, Normalize(message) ?? DefaultErrorMessage);
    }

    public static TimeoutException Timeout()
    {
        return new TimeoutException(TimeoutMessage);
    }

    private static string ExtractMessage(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return string.Empty;

        var trimmed = content.Trim();

        try
        {
            using var document = JsonDocument.Parse(trimmed);
            if (document.RootElement.ValueKind == JsonValueKind.Object &&
                document.RootElement.TryGetProperty("message", out var messageElement) &&
                messageElement.ValueKind == JsonValueKind.String)
            {
                return Normalize(messageElement.GetString()) ?? string.Empty;
            }

            if (document.RootElement.ValueKind == JsonValueKind.String)
                return Normalize(document.RootElement.GetString()) ?? string.Empty;
        }
        catch (JsonException)
        {
        }

        return Normalize(trimmed) ?? string.Empty;
    }

    private static string BuildDefaultMessage(HttpStatusCode statusCode)
    {
        return statusCode switch
        {
            HttpStatusCode.BadRequest => "Certaines informations sont invalides.",
            HttpStatusCode.Unauthorized => "Vous devez vous connecter pour continuer.",
            HttpStatusCode.Forbidden => "Vous n'avez pas l'autorisation d'effectuer cette action.",
            HttpStatusCode.NotFound => "La ressource demandee est introuvable.",
            HttpStatusCode.Conflict => "Cette action ne peut pas etre effectuee pour le moment.",
            HttpStatusCode.RequestTimeout => TimeoutMessage,
            HttpStatusCode.GatewayTimeout => TimeoutMessage,
            _ when (int)statusCode >= 500 => "Une erreur serveur est survenue. Reessayez plus tard.",
            _ => DefaultErrorMessage
        };
    }

    private static string? Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return WhitespaceRegex().Replace(value, " ").Trim();
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();
}
