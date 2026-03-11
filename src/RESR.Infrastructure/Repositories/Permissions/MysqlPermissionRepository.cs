using RESR.Core.Controllers.Permissions.Factories;
using RESR.Core.Controllers.Permissions.Ports;
using RESR.Models.Permissions;
using MySql.Data.MySqlClient;
using System.Data.Common;

namespace RESR.Infrastructure.Permissions;

public sealed class MySqlPermissionRepository : IPermissionRepository
{
    private readonly Func<DbConnection> _connectionFactory;
    private readonly IPermissionFactory _permissionFactory;

    public MySqlPermissionRepository(string connectionString, IPermissionFactory permissionFactory)
    {
        _connectionFactory = () => new MySqlConnection(connectionString);
        _permissionFactory = permissionFactory;
    }

    internal MySqlPermissionRepository(Func<DbConnection> connectionFactory, IPermissionFactory permissionFactory)
    {
        _connectionFactory = connectionFactory;
        _permissionFactory = permissionFactory;
    }

    public async Task<IReadOnlyList<Permission>> GetAllAsync(CancellationToken ct)
    {
        const string sql = """
        SELECT id_permission, name, description
        FROM `permission`
        ORDER BY id_permission DESC
        """;

        var list = new List<Permission>();

        await using var conn = _connectionFactory();
        await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        await using var reader = await cmd.ExecuteReaderAsync(ct);

        while (await reader.ReadAsync(ct))
            list.Add(Map(reader));

        return list;
    }

    public async Task<Permission?> GetByIdAsync(int idPermission, CancellationToken ct)
    {
        const string sql = """
        SELECT id_permission, name, description
        FROM `permission`
        WHERE id_permission = @idPermission
        """;

        await using var conn = _connectionFactory();
        await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        AddParameter(cmd, "@idPermission", idPermission);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? Map(reader) : null;
    }

    private Permission Map(DbDataReader reader) =>
        _permissionFactory.Create(
            Convert.ToInt32(reader["id_permission"]),
            Convert.ToString(reader["name"]) ?? string.Empty,
            reader["description"] == DBNull.Value ? null : Convert.ToString(reader["description"])
        );

    private static void AddParameter(DbCommand cmd, string name, object? value)
    {
        var param = cmd.CreateParameter();
        param.ParameterName = name;
        param.Value = value ?? DBNull.Value;
        cmd.Parameters.Add(param);
    }
}
