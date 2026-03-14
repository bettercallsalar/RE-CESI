using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Moq;
using RESR.Core.Controllers.Resources;
using RESR.WebAPI.Files;

namespace RESR.WebAPI.Tests.Files;

public sealed class LocalResourceFileStorageTests
{
    [Fact]
    public async Task SaveAsync_PrefixesStoredFileNameWithResourceIdentifier()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), $"resr-storage-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempRoot);

        try
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    [LocalResourceFileStorage.UploadsRootConfigurationKey] = tempRoot
                })
                .Build();

            var environment = new Mock<IWebHostEnvironment>();
            environment.SetupGet(value => value.ContentRootPath).Returns("/unused");

            var storage = new LocalResourceFileStorage(configuration, environment.Object);

            var files = await storage.SaveAsync(
                10,
                7,
                new[]
                {
                    new ResourceFileUpload("cover.jpg", "image/jpeg", 4, new byte[] { 1, 2, 3, 4 })
                },
                CancellationToken.None);

            var file = Assert.Single(files);
            Assert.StartsWith("resource-10-", file.FileName, StringComparison.Ordinal);
            Assert.EndsWith(".jpg", file.FileName, StringComparison.OrdinalIgnoreCase);
            Assert.Equal($"/uploads/resources/10/{file.FileName}", file.Path);
            Assert.True(File.Exists(Path.Combine(tempRoot, "resources", "10", file.FileName)));
        }
        finally
        {
            if (Directory.Exists(tempRoot))
            {
                Directory.Delete(tempRoot, true);
            }
        }
    }
}
