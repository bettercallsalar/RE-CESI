using MySql.Data.MySqlClient;
using RESR.Core.Controllers.Follows.Ports;
using RESR.Models.Follows;
using System.Data.Common;

namespace RESR.Infrastructure.Follows;

public sealed class MySqlFollowsRepository : IFollowsRepository
{
    private readonly Func<DbConnection> _connectionFactory;


    public MySqlFollowsRepository(string connectionString)
    {
        _connectionFactory = () => new MySqlConnection(connectionString);
    }

    internal MySqlFollowsRepository(Func<DbConnection> connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<FollowUser>> GetAllFollowersAsync(int idUser, CancellationToken ct)
    {
        const string sql = """
        SELECT u.id_user, u.username, u.first_name
        FROM `follows` f
        INNER JOIN `user` u ON u.id_user = f.id_follower
        WHERE f.id_following = @idUser AND u.deleted_at IS NULL
        ORDER BY u.id_user DESC
        """;

        var list = new List<FollowUser>();

        await using var conn = _connectionFactory();
        await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        AddParameter(cmd, "@idUser", idUser);
        await using var reader = await cmd.ExecuteReaderAsync(ct);

        while (await reader.ReadAsync(ct))
            list.Add(MapUser(reader));

        return list;
    }

    public async Task<IReadOnlyList<FollowUser>> GetAllFollowingAsync(int idUser, CancellationToken ct)
    {
        const string sql = """
        SELECT u.id_user, u.username, u.first_name
        FROM `follows` f
        INNER JOIN `user` u ON u.id_user = f.id_following
        WHERE f.id_follower = @idUser AND u.deleted_at IS NULL
        ORDER BY u.id_user DESC
        """;

        var list = new List<FollowUser>();

        await using var conn = _connectionFactory();
        await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        AddParameter(cmd, "@idUser", idUser);
        await using var reader = await cmd.ExecuteReaderAsync(ct);

        while (await reader.ReadAsync(ct))
            list.Add(MapUser(reader));

        return list;
    }

    public async Task<bool> ExistsAsync(int idFollower, int idFollowing, CancellationToken ct)
    {
        const string sql = """
        SELECT 1
        FROM `follows`
        WHERE id_follower = @idFollower AND id_following = @idFollowing
        LIMIT 1
        """;

        await using var conn = _connectionFactory();
        await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        AddParameter(cmd, "@idFollower", idFollower);
        AddParameter(cmd, "@idFollowing", idFollowing);

        var result = await cmd.ExecuteScalarAsync(ct);
        return result is not null && result != DBNull.Value;
    }

    public async Task<bool> CreateAsync(int idFollower, int idFollowing, CancellationToken ct)
    {
        const string sql = """
        INSERT IGNORE INTO `follows` (id_follower, id_following)
        VALUES (@idFollower, @idFollowing)
        """;

        await using var conn = _connectionFactory();
        await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        AddParameter(cmd, "@idFollower", idFollower);
        AddParameter(cmd, "@idFollowing", idFollowing);

        var rows = await cmd.ExecuteNonQueryAsync(ct);
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int idFollower, int idFollowing, CancellationToken ct)
    {
        const string sql = """
        DELETE FROM `follows`
        WHERE id_follower = @idFollower AND id_following = @idFollowing
        """;

        await using var conn = _connectionFactory();
        await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        AddParameter(cmd, "@idFollower", idFollower);
        AddParameter(cmd, "@idFollowing", idFollowing);

        var rows = await cmd.ExecuteNonQueryAsync(ct);
        return rows > 0;
    }


    private static FollowUser MapUser(DbDataReader reader) =>
        new()
        {
            IdUser = Convert.ToInt32(reader["id_user"]),
            Username = Convert.ToString(reader["username"]) ?? string.Empty,
            FirstName = Convert.ToString(reader["first_name"]) ?? string.Empty
        };

    private static void AddParameter(DbCommand cmd, string name, object? value)
    {
        var param = cmd.CreateParameter();
        param.ParameterName = name;
        param.Value = value ?? DBNull.Value;
        cmd.Parameters.Add(param);
    }
}
