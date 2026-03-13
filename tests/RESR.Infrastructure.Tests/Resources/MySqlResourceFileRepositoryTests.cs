using System.Data;
using System.Data.Common;
using RESR.Infrastructure.Resources;
using RESR.Infrastructure.Tests.DbFakes;
using RESR.Models.Resources;

namespace RESR.Infrastructure.Tests.Resources;

public sealed class MySqlResourceFileRepositoryTests
{
    [Fact]
    public async Task GetByResourceIdsAsync_ReturnsFilesGroupedByResource()
    {
        var table = new DataTable();
        table.Columns.Add("id_file", typeof(int));
        table.Columns.Add("file_name", typeof(string));
        table.Columns.Add("original_name", typeof(string));
        table.Columns.Add("mime_type", typeof(string));
        table.Columns.Add("size", typeof(int));
        table.Columns.Add("path", typeof(string));
        table.Columns.Add("created_at", typeof(DateTime));
        table.Columns.Add("created_by", typeof(string));
        table.Columns.Add("updated_at", typeof(DateTime));
        table.Columns.Add("updated_by", typeof(string));
        table.Columns.Add("id_ressource", typeof(int));
        table.Rows.Add(1, "file-1.jpg", "cover.jpg", "image/jpeg", 1234, "/uploads/resources/10/file-1.jpg", DateTime.UtcNow, "7", DBNull.Value, DBNull.Value, 10);
        table.Rows.Add(2, "file-2.jpg", "gallery.jpg", "image/jpeg", 2222, "/uploads/resources/10/file-2.jpg", DateTime.UtcNow, "7", DBNull.Value, DBNull.Value, 10);

        var repo = CreateRepo(new FakeDbCommand
        {
            ExecuteReaderHandler = _ => table.CreateDataReader()
        });

        var result = await repo.GetByResourceIdsAsync(new[] { 10 }, CancellationToken.None);

        Assert.True(result.ContainsKey(10));
        Assert.Equal(2, result[10].Count);
        Assert.Equal("cover.jpg", result[10][0].OriginalName);
    }

    [Fact]
    public async Task ReplaceForResourceAsync_DeletesThenInserts()
    {
        var deleteCmd = new FakeDbCommand { ExecuteNonQueryHandler = _ => 1 };
        var insertCmd = new FakeDbCommand { ExecuteNonQueryHandler = _ => 1 };
        var repo = CreateRepo(deleteCmd, insertCmd);

        await repo.ReplaceForResourceAsync(10, new[]
        {
            new ResourceFile
            {
                IdFile = 0,
                FileName = "file-1.jpg",
                OriginalName = "cover.jpg",
                MimeType = "image/jpeg",
                Size = 1234,
                Path = "/uploads/resources/10/file-1.jpg",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "7",
                UpdatedAt = null,
                UpdatedBy = null,
                IdResource = 10
            }
        }, CancellationToken.None);

        Assert.Contains("DELETE FROM file", deleteCmd.CommandText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("INSERT INTO file", insertCmd.CommandText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("@id_ressource", insertCmd.Parameters.Cast<DbParameter>().Select(parameter => parameter.ParameterName));
    }

    private static MySqlResourceFileRepository CreateRepo(params FakeDbCommand[] commands)
    {
        DbConnection ConnectionFactory() => new FakeDbConnection(commands);
        return new MySqlResourceFileRepository(ConnectionFactory);
    }
}
