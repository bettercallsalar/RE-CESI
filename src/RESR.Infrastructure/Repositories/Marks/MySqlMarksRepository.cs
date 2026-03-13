using System.Data.Common;
using MySql.Data.MySqlClient;
using RESR.Core.Controllers.Marks.Ports;
using RESR.Models.Marks;

namespace RESR.Infrastructure.Marks;

public sealed class MySqlMarksRepository : IMarksRepository
{
    private readonly Func<DbConnection> _connectionFactory;

    public MySqlMarksRepository(string connectionString)
    {
        _connectionFactory = () => new MySqlConnection(connectionString);
    }

    internal MySqlMarksRepository(Func<DbConnection> connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<bool> ResourceExistsAsync(int idResource, CancellationToken ct)
    {
        const string sql = """
        SELECT COUNT(*)
        FROM `resource`
        WHERE id_ressource = @idResource AND deleted_at IS NULL
        """;

        await using var conn = _connectionFactory();
        await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        AddParameter(cmd, "@idResource", idResource);

        var result = await cmd.ExecuteScalarAsync(ct);
        return Convert.ToInt32(result) > 0;
    }

    public async Task<Mark> MarkAsFavoriteAsync(int idResource, int idUser, CancellationToken ct)
    {
        const string sql = """
        INSERT INTO `mark` (is_favorite, is_read_later, id_ressource, id_user)
        VALUES (1, 0, @idResource, @idUser)
        ON DUPLICATE KEY UPDATE is_favorite = 1
        """;

        await using var conn = _connectionFactory();
        await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        AddParameter(cmd, "@idResource", idResource);
        AddParameter(cmd, "@idUser", idUser);
        await cmd.ExecuteNonQueryAsync(ct);

        return await GetByUserAndResourceAsync(conn, idResource, idUser, ct)
            ?? throw new InvalidOperationException("Mark not found after update");
    }

    public async Task<bool> UnmarkAsFavoriteAsync(int idResource, int idUser, CancellationToken ct)
    {
        const string updateSql = """
        UPDATE `mark`
        SET is_favorite = 0
        WHERE id_ressource = @idResource AND id_user = @idUser AND is_favorite = 1
        """;

        const string cleanupSql = """
        DELETE FROM `mark`
        WHERE id_ressource = @idResource AND id_user = @idUser AND is_favorite = 0 AND is_read_later = 0
        """;

        await using var conn = _connectionFactory();
        await conn.OpenAsync(ct);

        await using var updateCmd = conn.CreateCommand();
        updateCmd.CommandText = updateSql;
        AddParameter(updateCmd, "@idResource", idResource);
        AddParameter(updateCmd, "@idUser", idUser);

        var rows = await updateCmd.ExecuteNonQueryAsync(ct);
        if (rows == 0)
            return false;

        await using var cleanupCmd = conn.CreateCommand();
        cleanupCmd.CommandText = cleanupSql;
        AddParameter(cleanupCmd, "@idResource", idResource);
        AddParameter(cleanupCmd, "@idUser", idUser);
        await cleanupCmd.ExecuteNonQueryAsync(ct);

        return true;
    }

    public async Task<Mark> MarkAsReadLaterAsync(int idResource, int idUser, CancellationToken ct)
    {
        const string sql = """
        INSERT INTO `mark` (is_favorite, is_read_later, id_ressource, id_user)
        VALUES (0, 1, @idResource, @idUser)
        ON DUPLICATE KEY UPDATE is_read_later = 1
        """;

        await using var conn = _connectionFactory();
        await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        AddParameter(cmd, "@idResource", idResource);
        AddParameter(cmd, "@idUser", idUser);
        await cmd.ExecuteNonQueryAsync(ct);

        return await GetByUserAndResourceAsync(conn, idResource, idUser, ct)
            ?? throw new InvalidOperationException("Mark not found after update");
    }

    public async Task<bool> UnmarkAsReadLaterAsync(int idResource, int idUser, CancellationToken ct)
    {
        const string updateSql = """
        UPDATE `mark`
        SET is_read_later = 0
        WHERE id_ressource = @idResource AND id_user = @idUser AND is_read_later = 1
        """;

        const string cleanupSql = """
        DELETE FROM `mark`
        WHERE id_ressource = @idResource AND id_user = @idUser AND is_favorite = 0 AND is_read_later = 0
        """;

        await using var conn = _connectionFactory();
        await conn.OpenAsync(ct);

        await using var updateCmd = conn.CreateCommand();
        updateCmd.CommandText = updateSql;
        AddParameter(updateCmd, "@idResource", idResource);
        AddParameter(updateCmd, "@idUser", idUser);

        var rows = await updateCmd.ExecuteNonQueryAsync(ct);
        if (rows == 0)
            return false;

        await using var cleanupCmd = conn.CreateCommand();
        cleanupCmd.CommandText = cleanupSql;
        AddParameter(cleanupCmd, "@idResource", idResource);
        AddParameter(cleanupCmd, "@idUser", idUser);
        await cleanupCmd.ExecuteNonQueryAsync(ct);

        return true;
    }

    public async Task<IReadOnlyList<Mark>> GetFavoriteRessourcesAsync(int idUser, CancellationToken ct)
    {
        const string sql = """
        SELECT id_mark, is_favorite, is_read_later, id_ressource, id_user
        FROM `mark`
        WHERE id_user = @idUser AND is_favorite = 1
        ORDER BY id_mark DESC
        """;

        var list = new List<Mark>();

        await using var conn = _connectionFactory();
        await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        AddParameter(cmd, "@idUser", idUser);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
            list.Add(Map(reader));

        return list;
    }

    public async Task<IReadOnlyList<Mark>> GetReadLaterRessourcesAsync(int idUser, CancellationToken ct)
    {
        const string sql = """
        SELECT id_mark, is_favorite, is_read_later, id_ressource, id_user
        FROM `mark`
        WHERE id_user = @idUser AND is_read_later = 1
        ORDER BY id_mark DESC
        """;

        var list = new List<Mark>();

        await using var conn = _connectionFactory();
        await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        AddParameter(cmd, "@idUser", idUser);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
            list.Add(Map(reader));

        return list;
    }

    public async Task<Mark?> GetFavoriteRessourceAsync(int idResource, int idUser, CancellationToken ct)
    {
        const string sql = """
        SELECT id_mark, is_favorite, is_read_later, id_ressource, id_user
        FROM `mark`
        WHERE id_user = @idUser AND id_ressource = @idResource AND is_favorite = 1
        """;

        await using var conn = _connectionFactory();
        await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        AddParameter(cmd, "@idUser", idUser);
        AddParameter(cmd, "@idResource", idResource);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? Map(reader) : null;
    }

    public async Task<Mark?> GetReadLaterRessourceAsync(int idResource, int idUser, CancellationToken ct)
    {
        const string sql = """
        SELECT id_mark, is_favorite, is_read_later, id_ressource, id_user
        FROM `mark`
        WHERE id_user = @idUser AND id_ressource = @idResource AND is_read_later = 1
        """;

        await using var conn = _connectionFactory();
        await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        AddParameter(cmd, "@idUser", idUser);
        AddParameter(cmd, "@idResource", idResource);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? Map(reader) : null;
    }

    private async Task<Mark?> GetByUserAndResourceAsync(DbConnection conn, int idResource, int idUser, CancellationToken ct)
    {
        const string sql = """
        SELECT id_mark, is_favorite, is_read_later, id_ressource, id_user
        FROM `mark`
        WHERE id_user = @idUser AND id_ressource = @idResource
        """;

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        AddParameter(cmd, "@idUser", idUser);
        AddParameter(cmd, "@idResource", idResource);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? Map(reader) : null;
    }

    private static Mark Map(DbDataReader reader) =>
        new()
        {
            IdMark = Convert.ToInt32(reader["id_mark"]),
            IsFavorite = Convert.ToBoolean(reader["is_favorite"]),
            IsReadLater = Convert.ToBoolean(reader["is_read_later"]),
            IdRessource = Convert.ToInt32(reader["id_ressource"]),
            IdUser = Convert.ToInt32(reader["id_user"])
        };

    private static void AddParameter(DbCommand cmd, string name, object? value)
    {
        var param = cmd.CreateParameter();
        param.ParameterName = name;
        param.Value = value ?? DBNull.Value;
        cmd.Parameters.Add(param);
    }
}
