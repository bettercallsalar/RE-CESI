using System.IO;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;
using RESR.Models.Resources;

namespace RESR.MAUI.Services;

internal static class ApiEndpoints
{
    public static Uri ResolveBaseAddress()
    {
        var host = DeviceInfo.Current.Platform == DevicePlatform.Android
            ? "10.0.2.2"
            : "localhost";

        return new Uri($"http://{host}:8080/");
    }

    public static Uri? ResolvePreferredImageUri(IReadOnlyList<ResourceFileResponse>? files, int? defaultImageId)
    {
        var file = ResolvePreferredImage(files, defaultImageId);
        if (file is null || string.IsNullOrWhiteSpace(file.Path))
            return null;

        var path = file.Path.Trim();

        if (Uri.TryCreate(path, UriKind.Absolute, out var absoluteUri))
            return absoluteUri;

        return Uri.TryCreate(ResolveBaseAddress(), path, out var relativeUri)
            ? relativeUri
            : null;
    }

    public static ImageSource? CreateCachedImageSource(IReadOnlyList<ResourceFileResponse>? files, int? defaultImageId)
    {
        var imageUri = ResolvePreferredImageUri(files, defaultImageId);
        if (imageUri is null)
            return null;

        return new UriImageSource
        {
            Uri = imageUri,
            CachingEnabled = true,
            CacheValidity = TimeSpan.FromHours(12)
        };
    }

    private static ResourceFileResponse? ResolvePreferredImage(IReadOnlyList<ResourceFileResponse>? files, int? defaultImageId)
    {
        if (files is null || files.Count == 0)
            return null;

        if (defaultImageId is int imageId)
        {
            var defaultImage = files.FirstOrDefault(file => file.IdFile == imageId && IsImage(file));
            if (defaultImage is not null)
                return defaultImage;
        }

        return files.FirstOrDefault(IsImage);
    }

    private static bool IsImage(ResourceFileResponse file)
    {
        if (!string.IsNullOrWhiteSpace(file.MimeType))
            return file.MimeType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);

        var extension = Path.GetExtension(file.FileName);

        return extension.Equals(".png", StringComparison.OrdinalIgnoreCase) ||
               extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
               extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase) ||
               extension.Equals(".webp", StringComparison.OrdinalIgnoreCase) ||
               extension.Equals(".gif", StringComparison.OrdinalIgnoreCase) ||
               extension.Equals(".bmp", StringComparison.OrdinalIgnoreCase);
    }
}
