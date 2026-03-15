using System.Net;
using System.Text.RegularExpressions;

namespace RESR.MAUI;

internal static class DisplayText
{
    public static string FirstNonEmpty(params string?[] values)
    {
        foreach (var value in values)
        {
            var normalized = Normalize(value);
            if (!string.IsNullOrWhiteSpace(normalized))
                return normalized;
        }

        return string.Empty;
    }

    public static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var withoutTags = Regex.Replace(value, "<[^>]+>", " ");
        var decoded = WebUtility.HtmlDecode(withoutTags);

        return Regex.Replace(decoded, @"\s+", " ").Trim();
    }

    public static string ToExcerpt(string? value, int maxLength)
    {
        var normalized = Normalize(value);
        if (normalized.Length <= maxLength)
            return normalized;

        return normalized[..Math.Max(0, maxLength - 3)].TrimEnd() + "...";
    }
}
