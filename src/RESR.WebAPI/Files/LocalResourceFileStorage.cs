using RESR.Core.Controllers.Resources;
using RESR.Core.Controllers.Resources.Ports;
using RESR.Models.Resources;

namespace RESR.WebAPI.Files;

public sealed class LocalResourceFileStorage : IResourceFileStorage
{
    public const string PublicRequestPath = "/uploads";
    public const string UploadsRootConfigurationKey = "FileStorage:UploadsRoot";

    private readonly string _rootDirectory;

    public LocalResourceFileStorage(IConfiguration configuration, IWebHostEnvironment environment)
    {
        _rootDirectory = Path.Combine(GetUploadsRootDirectory(configuration, environment), "resources");
    }

    public static string GetUploadsRootDirectory(IConfiguration configuration, IWebHostEnvironment environment)
    {
        var configuredPath = configuration[UploadsRootConfigurationKey];
        if (!string.IsNullOrWhiteSpace(configuredPath))
        {
            return Path.IsPathRooted(configuredPath)
                ? configuredPath
                : Path.GetFullPath(Path.Combine(environment.ContentRootPath, configuredPath));
        }

        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        if (!string.IsNullOrWhiteSpace(localAppData))
            return Path.Combine(localAppData, "RESR", "uploads");

        var userHome = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return Path.Combine(userHome, ".resr", "uploads");
    }

    public async Task<IReadOnlyList<ResourceFile>> SaveAsync(int idResource, int idUser, IReadOnlyList<ResourceFileUpload> uploads, CancellationToken ct)
    {
        var resourceDirectory = Path.Combine(_rootDirectory, idResource.ToString());
        Directory.CreateDirectory(resourceDirectory);

        var createdAt = DateTime.UtcNow;
        var files = new List<ResourceFile>(uploads.Count);

        foreach (var upload in uploads)
        {
            var extension = Path.GetExtension(upload.OriginalName);
            var fileName = $"resource-{idResource}-{Guid.NewGuid():N}{extension}";
            var fullPath = Path.Combine(resourceDirectory, fileName);

            await File.WriteAllBytesAsync(fullPath, upload.Content, ct);

            files.Add(new ResourceFile
            {
                FileName = fileName,
                OriginalName = upload.OriginalName,
                MimeType = upload.MimeType,
                Size = upload.Size,
                Path = $"{PublicRequestPath}/resources/{idResource}/{fileName}",
                CreatedAt = createdAt,
                CreatedBy = idUser.ToString(),
                UpdatedAt = null,
                UpdatedBy = null,
                IdResource = idResource
            });
        }

        return files;
    }

    public Task DeleteAsync(IReadOnlyList<ResourceFile> files, CancellationToken ct)
    {
        foreach (var file in files)
        {
            var relativePath = file.Path.Replace(PublicRequestPath, string.Empty, StringComparison.OrdinalIgnoreCase)
                .TrimStart('/')
                .Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.Combine(_rootDirectory, "..", relativePath);
            fullPath = Path.GetFullPath(fullPath);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }

        return Task.CompletedTask;
    }
}
