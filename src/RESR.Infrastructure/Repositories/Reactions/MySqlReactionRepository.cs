using System.Data.Common;
using MySql.Data.MySqlClient;
using RESR.Core.Controllers.Reactions.Factories;
using RESR.Core.Controllers.Reactions.Ports;
using RESR.Models.Reactions;

namespace RESR.Infrastructure.Reactions;

public sealed class MySqlReactionRepository : IReactionRepository
{
    private readonly Func<DbConnection> _connectionFactory;
    private readonly IReactionFactory _reactionFactory;

    public MySqlReactionRepository(string connectionString, IReactionFactory reactionFactory)
    {
        _connectionFactory = () => new MySqlConnection(connectionString);
        _reactionFactory = reactionFactory;
    }

    internal MySqlReactionRepository(Func<DbConnection> connectionFactory, IReactionFactory reactionFactory)
    {
        _connectionFactory = connectionFactory;
        _reactionFactory = reactionFactory;
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

    public async Task<bool> UserExistsAsync(int idUser, CancellationToken ct)
    {
        const string sql = """
        SELECT COUNT(*)
        FROM `user`
        WHERE id_user = @idUser AND deleted_at IS NULL
        """;

        await using var conn = _connectionFactory();
        await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        AddParameter(cmd, "@idUser", idUser);

        var result = await cmd.ExecuteScalarAsync(ct);
        return Convert.ToInt32(result) > 0;
    }

    public async Task<IReadOnlyList<Reaction>> GetByResourceIdAsync(int idResource, CancellationToken ct)
    {
        const string sql = """
        SELECT r.id_reaction, r.name, r.id_ressource, r.id_user, u.username, u.first_name
        FROM `reaction` r
        INNER JOIN `user` u ON u.id_user = r.id_user
        WHERE id_ressource = @idResource
        ORDER BY r.id_reaction ASC
        """;

        var reactions = new List<Reaction>();

        await using var conn = _connectionFactory();
        await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        AddParameter(cmd, "@idResource", idResource);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
            reactions.Add(Map(reader));

        return reactions;
    }

    public async Task<IReadOnlyList<Reaction>> GetByUserIdAsync(int idUser, CancellationToken ct)
    {
        const string sql = """
        SELECT r.id_reaction, r.name, r.id_ressource, r.id_user, u.username, u.first_name
        FROM `reaction` r
        INNER JOIN `user` u ON u.id_user = r.id_user
        WHERE r.id_user = @idUser
        ORDER BY r.id_reaction ASC
        """;

        var reactions = new List<Reaction>();

        await using var conn = _connectionFactory();
        await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        AddParameter(cmd, "@idUser", idUser);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
            reactions.Add(Map(reader));

        return reactions;
    }

    public async Task<Reaction?> GetByIdAsync(int idReaction, CancellationToken ct)
    {
        await using var conn = _connectionFactory();
        await conn.OpenAsync(ct);

        return await GetByIdAsync(conn, idReaction, ct);
    }

    public async Task<Reaction?> GetByResourceAndUserAsync(int idResource, int idUser, CancellationToken ct)
    {
        const string sql = """
        SELECT r.id_reaction, r.name, r.id_ressource, r.id_user, u.username, u.first_name
        FROM `reaction` r
        INNER JOIN `user` u ON u.id_user = r.id_user
        WHERE r.id_ressource = @idResource AND r.id_user = @idUser
        """;

        await using var conn = _connectionFactory();
        await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        AddParameter(cmd, "@idResource", idResource);
        AddParameter(cmd, "@idUser", idUser);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? Map(reader) : null;
    }

    public async Task<Reaction> CreateAsync(Reaction reaction, CancellationToken ct)
    {
        const string insertSql = """
        INSERT INTO `reaction` (`name`, `id_ressource`, `id_user`)
        VALUES (@name, @idResource, @idUser);
        SELECT LAST_INSERT_ID();
        """;

        await using var conn = _connectionFactory();
        await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = insertSql;
        AddParameter(cmd, "@name", reaction.Name);
        AddParameter(cmd, "@idResource", reaction.IdResource);
        AddParameter(cmd, "@idUser", reaction.IdUser);

        var id = Convert.ToInt32(await cmd.ExecuteScalarAsync(ct));
        return await GetByIdAsync(conn, id, ct)
            ?? throw new InvalidOperationException("Reaction not found after creation");
    }

    public async Task<Reaction> UpdateNameAsync(int idReaction, string name, CancellationToken ct)
    {
        const string updateSql = """
        UPDATE `reaction`
        SET name = @name
        WHERE id_reaction = @idReaction
        """;

        await using var conn = _connectionFactory();
        await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = updateSql;
        AddParameter(cmd, "@idReaction", idReaction);
        AddParameter(cmd, "@name", name);

        var rows = await cmd.ExecuteNonQueryAsync(ct);
        if (rows == 0)
            throw new InvalidOperationException("Reaction not found");

        return await GetByIdAsync(conn, idReaction, ct)
            ?? throw new InvalidOperationException("Reaction not found after update");
    }

    public async Task<bool> DeleteAsync(int idReaction, CancellationToken ct)
    {
        const string sql = """
        DELETE FROM `reaction`
        WHERE id_reaction = @idReaction
        """;

        await using var conn = _connectionFactory();
        await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        AddParameter(cmd, "@idReaction", idReaction);

        var rows = await cmd.ExecuteNonQueryAsync(ct);
        return rows > 0;
    }

    private Reaction Map(DbDataReader reader) =>
        _reactionFactory.CreateFromPersistence(
            Convert.ToInt32(reader["id_reaction"]),
            Convert.ToString(reader["name"]) ?? string.Empty,
            Convert.ToInt32(reader["id_ressource"]),
            Convert.ToInt32(reader["id_user"]),
            Convert.ToString(reader["username"]),
            Convert.ToString(reader["first_name"])
        );

    private async Task<Reaction?> GetByIdAsync(DbConnection conn, int idReaction, CancellationToken ct)
    {
        const string sql = """
        SELECT r.id_reaction, r.name, r.id_ressource, r.id_user, u.username, u.first_name
        FROM `reaction` r
        INNER JOIN `user` u ON u.id_user = r.id_user
        WHERE r.id_reaction = @idReaction
        """;

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        AddParameter(cmd, "@idReaction", idReaction);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? Map(reader) : null;
    }

    private static void AddParameter(DbCommand cmd, string name, object? value)
    {
        var parameter = cmd.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value ?? DBNull.Value;
        cmd.Parameters.Add(parameter);
    }
}
