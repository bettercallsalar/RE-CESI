using System.Data.Common;
using System.Text;
using MySql.Data.MySqlClient;
using RESR.Core.Controllers.Resources.Ports;
using RESR.Models.Resources;

namespace RESR.Infrastructure.Resources;

public sealed class MySqlResourceFileRepository : IResourceFileRepository
{
    private readonly Func<DbConnection> _connectionFactory;

    public MySqlResourceFileRepository(string connectionString)
    {
        _connectionFactory = () => new MySqlConnection(connectionString);
    }

    internal MySqlResourceFileRepository(Func<DbConnection> connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyDictionary<int, IReadOnlyList<ResourceFile>>> GetByResourceIdsAsync(IReadOnlyCollection<int> resourceIds, CancellationToken ct)
    {
        if (resourceIds.Count == 0)
            return new Dictionary<int, IReadOnlyList<ResourceFile>>();

        var sql = new StringBuilder("""
        SELECT
            id_file,
            file_name,
            original_name,
            mime_type,
            size,
            path,
            created_at,
            created_by,
            updated_at,
            updated_by,
            id_ressource
        FROM file
        WHERE id_ressource IN (
        """);

        await using var conn = _connectionFactory();
        await conn.OpenAsync(ct);
        await using var cmd = conn.CreateCommand();

        var parameterNames = new List<string>();
        var index = 0;
        foreach (var resourceId in resourceIds.Distinct())
        {
            var parameterName = $"@resource_{index++}";
            parameterNames.Add(parameterName);
            AddParameter(cmd, parameterName, resourceId);
        }

        sql.Append(string.Join(", ", parameterNames));
        sql.AppendLine(")");
        sql.AppendLine("ORDER BY id_file ASC");

        cmd.CommandText = sql.ToString();

        var files = new Dictionary<int, List<ResourceFile>>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            var file = Map(reader);
            if (!files.TryGetValue(file.IdResource, out var list))
            {
                list = [];
                files[file.IdResource] = list;
            }

            list.Add(file);
        }

        return files.ToDictionary(pair => pair.Key, pair => (IReadOnlyList<ResourceFile>)pair.Value);
    }

    public async Task<IReadOnlyList<ResourceFile>> ReplaceForResourceAsync(int idResource, IReadOnlyList<ResourceFile> files, CancellationToken ct)
    {
        await using var conn = _connectionFactory();
        await conn.OpenAsync(ct);

        await using var deleteCmd = conn.CreateCommand();
        deleteCmd.CommandText = "DELETE FROM file WHERE id_ressource = @id_ressource";
        AddParameter(deleteCmd, "@id_ressource", idResource);
        await deleteCmd.ExecuteNonQueryAsync(ct);

        if (files.Count == 0)
            return Array.Empty<ResourceFile>();

        var persistedFiles = new List<ResourceFile>(files.Count);

        foreach (var file in files)
        {
            await using var insertCmd = conn.CreateCommand();
            insertCmd.CommandText = """
            INSERT INTO file (file_name, original_name, mime_type, size, path, created_at, created_by, updated_at, updated_by, id_ressource)
            VALUES (@file_name, @original_name, @mime_type, @size, @path, @created_at, @created_by, @updated_at, @updated_by, @id_ressource);
            SELECT LAST_INSERT_ID();
            """;
            AddParameter(insertCmd, "@file_name", file.FileName);
            AddParameter(insertCmd, "@original_name", file.OriginalName);
            AddParameter(insertCmd, "@mime_type", file.MimeType);
            AddParameter(insertCmd, "@size", file.Size);
            AddParameter(insertCmd, "@path", file.Path);
            AddParameter(insertCmd, "@created_at", file.CreatedAt);
            AddParameter(insertCmd, "@created_by", (object?)file.CreatedBy ?? DBNull.Value);
            AddParameter(insertCmd, "@updated_at", (object?)file.UpdatedAt ?? DBNull.Value);
            AddParameter(insertCmd, "@updated_by", (object?)file.UpdatedBy ?? DBNull.Value);
            AddParameter(insertCmd, "@id_ressource", idResource);
            var idFile = Convert.ToInt32(await insertCmd.ExecuteScalarAsync(ct));
            persistedFiles.Add(new ResourceFile
            {
                IdFile = idFile,
                FileName = file.FileName,
                OriginalName = file.OriginalName,
                MimeType = file.MimeType,
                Size = file.Size,
                Path = file.Path,
                CreatedAt = file.CreatedAt,
                CreatedBy = file.CreatedBy,
                UpdatedAt = file.UpdatedAt,
                UpdatedBy = file.UpdatedBy,
                IdResource = idResource
            });
        }

        return persistedFiles;
    }

    public async Task DeleteForResourceAsync(int idResource, CancellationToken ct)
    {
        await using var conn = _connectionFactory();
        await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM file WHERE id_ressource = @id_ressource";
        AddParameter(cmd, "@id_ressource", idResource);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    private static ResourceFile Map(DbDataReader reader)
    {
        return new ResourceFile
        {
            IdFile = Convert.ToInt32(reader["id_file"]),
            FileName = Convert.ToString(reader["file_name"]) ?? string.Empty,
            OriginalName = Convert.ToString(reader["original_name"]) ?? string.Empty,
            MimeType = Convert.ToString(reader["mime_type"]) ?? string.Empty,
            Size = Convert.ToInt32(reader["size"]),
            Path = Convert.ToString(reader["path"]) ?? string.Empty,
            CreatedAt = Convert.ToDateTime(reader["created_at"]),
            CreatedBy = reader["created_by"] == DBNull.Value ? null : Convert.ToString(reader["created_by"]),
            UpdatedAt = reader["updated_at"] == DBNull.Value ? null : Convert.ToDateTime(reader["updated_at"]),
            UpdatedBy = reader["updated_by"] == DBNull.Value ? null : Convert.ToString(reader["updated_by"]),
            IdResource = Convert.ToInt32(reader["id_ressource"])
        };
    }

    private static void AddParameter(DbCommand cmd, string name, object? value)
    {
        var parameter = cmd.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value ?? DBNull.Value;
        cmd.Parameters.Add(parameter);
    }
}
